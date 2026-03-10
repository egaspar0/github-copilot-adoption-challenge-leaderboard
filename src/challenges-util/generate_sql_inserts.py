#!/usr/bin/env python3
"""
GitHub Copilot Challenge Markdown to SQL Insert Generator

This script reads markdown files with YAML front matter from the challenges-markdown folder,
converts them to HTML, handles binary answer patterns, and generates SQL INSERT statements.
"""

import os
import re
import sys
import subprocess
from pathlib import Path
from typing import Dict, List, Tuple, Optional
import html

# For markdown parsing
try:
    import markdown
    from markdown.extensions import tables, fenced_code
except ImportError:
    print("Error: markdown package not found. Installing...")
    import subprocess
    subprocess.check_call([sys.executable, "-m", "pip", "install", "markdown"])
    import markdown
    from markdown.extensions import tables, fenced_code

# For YAML parsing
try:
    import yaml
except ImportError:
    print("Error: pyyaml package not found. Installing...")
    import subprocess
    subprocess.check_call([sys.executable, "-m", "pip", "install", "pyyaml"])
    import yaml


class ChallengeProcessor:
    """Process markdown challenge files and generate SQL INSERT statements."""
    
    def __init__(self, challenges_dir: str, output_file: str):
        """
        Initialize the processor.
        
        Args:
            challenges_dir: Path to the challenges-markdown directory
            output_file: Path to the output SQL file
        """
        self.challenges_dir = Path(challenges_dir)
        self.output_file = Path(output_file)
        self.challenges: List[Dict] = []
    
    def extract_front_matter(self, content: str) -> Tuple[Dict, str]:
        """
        Extract YAML front matter from markdown content.
        
        Args:
            content: Full markdown file content
            
        Returns:
            Tuple of (front_matter_dict, body_content)
        """
        # Check if content starts with ---
        if not content.startswith('---'):
            return {}, content
        
        # Find the second --- that closes the front matter
        lines = content.split('\n')
        front_matter_end = None
        
        for i in range(1, len(lines)):
            if lines[i].strip() == '---':
                front_matter_end = i
                break
        
        if front_matter_end is None:
            return {}, content
        
        front_matter_str = '\n'.join(lines[1:front_matter_end])
        body = '\n'.join(lines[front_matter_end + 1:])
        
        try:
            # Try standard YAML parsing first
            front_matter = yaml.safe_load(front_matter_str)
            if front_matter is None:
                front_matter = {}
            return front_matter, body
        except yaml.YAMLError:
            # Fallback: parse manually for simple key: value pairs
            front_matter = {}
            for line in lines[1:front_matter_end]:
                if ':' in line:
                    key, value = line.split(':', 1)
                    front_matter[key.strip()] = value.strip()
            return front_matter, body
    
    def convert_binary_answers_in_html(self, html_content: str, title: str) -> str:
        """
        Transform binary answer prompts in HTML to radio button inputs.
        
        This handles:
        1. Text patterns like "Yes / No", "True/False" after questions
        2. Checkbox patterns with consecutive "- [ ] Yes" and "- [ ] No" lines
        
        Args:
            html_content: HTML content
            title: Title for generating radio input name (slugified)
            
        Returns:
            Modified HTML with radio inputs
        """
        slug = self.slugify(title)
        question_counter = 1
        
        # Pattern 1: Handle checkbox-style radio button pairs that markdown creates as lists
        # Look for patterns like <li>[ ] Yes</li> followed by <li>[ ] No</li> or similar
        # But first convert checkbox patterns in the markdown before HTML conversion
        
        # For now, we'll handle this after HTML conversion by looking for list patterns
        # Pattern: <ul><li>[ ] Yes</li><li>[ ] No</li></ul> or similar
        
        # Pattern 1: Handle checkbox-style radio button pairs
        # Each Yes/No pair gets its own incrementing question number.
        checkbox_patterns = [
            (r'<li>\[\s*\]\s*Yes\s*</li>\s*<li>\[\s*\]\s*No\s*</li>',  'yes', 'no'),
            (r'<li>\[\s*\]\s*No\s*</li>\s*<li>\[\s*\]\s*Yes\s*</li>',  'no',  'yes'),
            (r'<li>\[\s*\]\s*True\s*</li>\s*<li>\[\s*\]\s*False\s*</li>', 'true',  'false'),
            (r'<li>\[\s*\]\s*False\s*</li>\s*<li>\[\s*\]\s*True\s*</li>', 'false', 'true'),
        ]

        for pattern, first_val, second_val in checkbox_patterns:
            first_label  = first_val.capitalize()
            second_label = second_val.capitalize()
            # Each match in the document is a separate question — increment per match.
            def make_replacement(m, _slug=slug, _fv=first_val, _fl=first_label, _sv=second_val, _sl=second_label):
                nonlocal question_counter
                q = f"{_slug}-q{question_counter}"
                question_counter += 1
                return (f'<label><input type="radio" name="{q}" value="{_fv}"> {_fl}</label>'
                        f'<label><input type="radio" name="{q}" value="{_sv}"> {_sl}</label>')
            html_content = re.sub(pattern, make_replacement, html_content)
        
        # Pattern 2: Handle text patterns like "Yes / No", "True/False" after paragraphs
        # Look for patterns like "Yes / No", "Yes/No", "No / Yes", "True / False", "True/False"
        text_patterns = [
            (r'Yes\s*/\s*No', f'<label><input type="radio" name="{slug}-q{question_counter}" value="yes"> Yes</label> / <label><input type="radio" name="{slug}-q{question_counter}" value="no"> No</label>'),
            (r'No\s*/\s*Yes', f'<label><input type="radio" name="{slug}-q{question_counter}" value="no"> No</label> / <label><input type="radio" name="{slug}-q{question_counter}" value="yes"> Yes</label>'),
            (r'True\s*/\s*False', f'<label><input type="radio" name="{slug}-q{question_counter}" value="true"> True</label> / <label><input type="radio" name="{slug}-q{question_counter}" value="false"> False</label>'),
            (r'False\s*/\s*True', f'<label><input type="radio" name="{slug}-q{question_counter}" value="false"> False</label> / <label><input type="radio" name="{slug}-q{question_counter}" value="true"> True</label>'),
        ]
        
        for pattern, replacement in text_patterns:
            # Use a counter to handle multiple questions
            matches = re.finditer(pattern, html_content)
            match_list = list(matches)
            # Replace in reverse order to maintain indices
            for match in reversed(match_list):
                qnum = question_counter
                question_counter += 1
                slug_q = f"{slug}-q{qnum}"
                
                if "Yes" in pattern or "yes" in replacement:
                    if "/" in pattern:
                        if "Yes" in pattern[:5]:
                            repl = f'<label><input type="radio" name="{slug_q}" value="yes"> Yes</label> / <label><input type="radio" name="{slug_q}" value="no"> No</label>'
                        else:
                            repl = f'<label><input type="radio" name="{slug_q}" value="no"> No</label> / <label><input type="radio" name="{slug_q}" value="yes"> Yes</label>'
                    else:
                        repl = replacement
                else:
                    if "/" in pattern:
                        if "True" in pattern[:5]:
                            repl = f'<label><input type="radio" name="{slug_q}" value="true"> True</label> / <label><input type="radio" name="{slug_q}" value="false"> False</label>'
                        else:
                            repl = f'<label><input type="radio" name="{slug_q}" value="false"> False</label> / <label><input type="radio" name="{slug_q}" value="true"> True</label>'
                    else:
                        repl = replacement
                
                html_content = html_content[:match.start()] + repl + html_content[match.end():]
        
        return html_content
    
    def slugify(self, text: str) -> str:
        """
        Convert text to a URL-friendly slug.
        
        Args:
            text: Text to slugify
            
        Returns:
            Slugified text (lowercase, alphanumeric, hyphens)
        """
        # Convert to lowercase
        slug = text.lower()
        # Replace spaces with hyphens
        slug = re.sub(r'\s+', '-', slug)
        # Keep only alphanumeric and hyphens
        slug = re.sub(r'[^a-z0-9-]', '', slug)
        # Replace multiple hyphens with single hyphen
        slug = re.sub(r'-+', '-', slug)
        # Remove leading/trailing hyphens
        slug = slug.strip('-')
        return slug
    
    def markdown_to_html(self, markdown_content: str) -> str:
        """
        Convert markdown to HTML using GitHub-flavored markdown rules.
        
        Args:
            markdown_content: Markdown content
            
        Returns:
            HTML content
        """
        # Configure markdown extensions for GitHub-flavored markdown
        extensions = [
            'fenced_code',
            'tables',
            'sane_lists',
            'nl2br'
        ]
        
        # Convert markdown to HTML
        html_content = markdown.markdown(
            markdown_content,
            extensions=extensions,
            extension_configs={
                'fenced_code': {
                    'lang_prefix': 'language-'
                }
            }
        )
        
        # Enhance code blocks with proper HTML structure
        # Replace markdown's code fence output with better formatting
        html_content = re.sub(
            r'<pre><code class="language-(\w+)">(.*?)</code></pre>',
            r'<pre><code class="\1">\2</code></pre>',
            html_content,
            flags=re.DOTALL
        )
        
        return html_content
    
    def escape_for_sql(self, text: str) -> str:
        """
        Escape text for SQL embedding.
        
        Rules:
        - Single quotes become doubled ('')
        - Newlines become spaces
        - Collapse multiple spaces
        
        Args:
            text: Text to escape
            
        Returns:
            Escaped text
        """
        # Replace single quotes with doubled single quotes
        text = text.replace("'", "''")
        
        # Replace newlines with spaces
        text = text.replace('\n', ' ')
        text = text.replace('\r', ' ')
        
        # Collapse multiple spaces into single space
        text = re.sub(r'\s+', ' ', text)
        
        # Strip leading/trailing whitespace
        text = text.strip()
        
        return text
    
    def process_file(self, filepath: Path) -> Optional[Dict]:
        """
        Process a single markdown file.
        
        Args:
            filepath: Path to the markdown file
            
        Returns:
            Dictionary with challenge data or None if processing fails
        """
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Extract front matter and body
            front_matter, body = self.extract_front_matter(content)
            
            # Get title (required)
            title = front_matter.get('Title')
            if not title:
                print(f"Warning: No title found in {filepath.name}, skipping")
                return None
            
            # Get ActivityId (optional, default to 12)
            activity_id = front_matter.get('ActivityId', 12)
            if not isinstance(activity_id, int):
                try:
                    activity_id = int(activity_id)
                except (ValueError, TypeError):
                    activity_id = 12
            
            # Convert markdown to HTML
            html_content = self.markdown_to_html(body)
            
            # Transform binary answers to radio inputs
            html_content = self.convert_binary_answers_in_html(html_content, title)
            
            # Escape for SQL
            escaped_html = self.escape_for_sql(html_content)
            
            return {
                'title': title,
                'activity_id': activity_id,
                'html': escaped_html
            }
        
        except Exception as e:
            print(f"Error processing {filepath.name}: {e}")
            return None
    
    def generate_sql_insert(self, title: str, html_content: str, activity_id: int) -> str:
        """
        Generate a SQL INSERT statement.
        
        Args:
            title: Challenge title
            html_content: HTML content (already escaped)
            activity_id: Activity ID
            
        Returns:
            SQL INSERT statement
        """
        # Escape title for SQL (single quotes doubled)
        escaped_title = title.replace("'", "''")
        
        sql = f"INSERT INTO [dbo].[Challenges] (Title, Content, PostedDate, ActivityId) VALUES (N'{escaped_title}', N'{html_content}', SYSDATETIME(), {activity_id});"
        
        return sql
    
    def process_all(self) -> None:
        """Process all markdown files and generate SQL output."""
        print(f"Reading markdown files from: {self.challenges_dir}")
        
        # Find all .md files
        md_files = sorted(self.challenges_dir.glob('*.md'))
        
        if not md_files:
            print(f"No markdown files found in {self.challenges_dir}")
            return
        
        print(f"Found {len(md_files)} markdown files")
        
        # Process each file
        for filepath in md_files:
            # Skip the generator script and other non-challenge files
            if filepath.name.startswith('generate_') or filepath.name == 'myinsert.sql':
                continue
            
            challenge = self.process_file(filepath)
            if challenge:
                self.challenges.append(challenge)
        
        if not self.challenges:
            print("No challenges were successfully processed")
            return
        
        print(f"Successfully processed {len(self.challenges)} challenges")
        
        # Sort by ActivityId with custom order: 11, 10, 12
        activity_order = {11: 0, 10: 1, 12: 2}
        self.challenges.sort(key=lambda x: (activity_order.get(x['activity_id'], 999), x['activity_id']))
        
        # Generate SQL INSERT statements
        sql_statements = []
        for challenge in self.challenges:
            sql = self.generate_sql_insert(
                challenge['title'],
                challenge['html'],
                challenge['activity_id']
            )
            sql_statements.append(sql)
        
        # Write to output file
        self._write_output(sql_statements)
    
    def _write_output(self, sql_statements: List[str]) -> None:
        """
        Write SQL statements to output file.
        
        Args:
            sql_statements: List of SQL INSERT statements
        """
        try:
            with open(self.output_file, 'w', encoding='utf-8') as f:
                for statement in sql_statements:
                    f.write(statement)
                    f.write('\n')
                # Leave a single blank line at the end
                f.write('\n')
            
            print(f"Successfully wrote {len(sql_statements)} SQL statements to {self.output_file}")
        
        except Exception as e:
            print(f"Error writing output file: {e}")


def main():
    """Main entry point."""
    # Define paths
    script_dir = Path(__file__).parent
    challenges_dir = script_dir.parent / 'challenges-markdown'
    output_file = script_dir / 'challenges-insert.sql'
    
    # Create processor and run
    processor = ChallengeProcessor(str(challenges_dir), str(output_file))
    processor.process_all()


if __name__ == '__main__':
    main()
