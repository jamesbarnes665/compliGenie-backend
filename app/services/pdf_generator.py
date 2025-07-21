import os
from datetime import datetime
from reportlab.lib.pagesizes import letter, A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import inch
from reportlab.platypus import (
    SimpleDocTemplate, 
    Paragraph, 
    Spacer, 
    PageBreak,
    Table,
    TableStyle,
    KeepTogether,
    ListFlowable,
    ListItem
)
from reportlab.lib.enums import TA_JUSTIFY, TA_LEFT, TA_CENTER
from reportlab.lib import colors
from reportlab.platypus.tableofcontents import TableOfContents
from reportlab.lib.pagesizes import letter
from io import BytesIO
import textwrap

class EnhancedPDFGenerator:
    """
    Enhanced PDF generator with proper text wrapping, margins, and formatting
    Fixes text cutoff issues and implements professional document structure
    """
    
    def __init__(self):
        self.styles = getSampleStyleSheet()
        self._create_custom_styles()
        
    def _create_custom_styles(self):
        """Create professional custom styles for the document"""
        # Main title style
        self.styles.add(ParagraphStyle(
            name='CustomTitle',
            parent=self.styles['Title'],
            fontSize=24,
            textColor=colors.HexColor('#2c3e50'),
            spaceAfter=30,
            alignment=TA_CENTER,
            leading=30
        ))
        
        # Section heading style
        self.styles.add(ParagraphStyle(
            name='SectionHeading',
            parent=self.styles['Heading1'],
            fontSize=16,
            textColor=colors.HexColor('#34495e'),
            spaceBefore=24,
            spaceAfter=12,
            leftIndent=0,
            keepWithNext=True,
            leading=20
        ))
        
        # Subsection heading style
        self.styles.add(ParagraphStyle(
            name='SubsectionHeading',
            parent=self.styles['Heading2'],
            fontSize=14,
            textColor=colors.HexColor('#7f8c8d'),
            spaceBefore=18,
            spaceAfter=10,
            leftIndent=0.25*inch,
            keepWithNext=True,
            leading=18
        ))
        
        # Body text style with proper justification
        self.styles.add(ParagraphStyle(
            name='BodyTextJustified',
            parent=self.styles['BodyText'],
            fontSize=11,
            alignment=TA_JUSTIFY,
            spaceBefore=6,
            spaceAfter=6,
            leftIndent=0,
            rightIndent=0,
            leading=16,  # Line spacing
            wordWrap='CJK'  # Better word wrapping
        ))
        
        # Bullet list style
        self.styles.add(ParagraphStyle(
            name='BulletList',
            parent=self.styles['BodyText'],
            fontSize=11,
            leftIndent=0.5*inch,
            spaceBefore=3,
            spaceAfter=3,
            bulletIndent=0.25*inch,
            leading=16
        ))

    def _add_page_numbers(self, canvas, doc):
        """Add page numbers and headers to each page"""
        canvas.saveState()
        
        # Add page number
        page_num = canvas.getPageNumber()
        text = f"Page {page_num}"
        canvas.setFont('Helvetica', 9)
        canvas.drawRightString(doc.width + doc.rightMargin, 0.5*inch, text)
        
        # Add header line
        canvas.setStrokeColor(colors.HexColor('#bdc3c7'))
        canvas.setLineWidth(0.5)
        canvas.line(doc.leftMargin, doc.height + doc.topMargin - 0.25*inch, 
                   doc.width + doc.leftMargin, doc.height + doc.topMargin - 0.25*inch)
        
        canvas.restoreState()

    def _wrap_text(self, text, max_width=80):
        """Ensure text wraps properly within margins"""
        if not text:
            return ""
        
        # Clean up text first
        text = text.strip().replace('\n\n', '|||PARAGRAPH|||')
        text = text.replace('\n', ' ')
        text = text.replace('|||PARAGRAPH|||', '\n\n')
        
        # Wrap long lines
        lines = text.split('\n')
        wrapped_lines = []
        
        for line in lines:
            if len(line) > max_width:
                wrapped = textwrap.fill(line, width=max_width, break_long_words=False)
                wrapped_lines.append(wrapped)
            else:
                wrapped_lines.append(line)
        
        return '\n'.join(wrapped_lines)

    def generate_policy_pdf(self, policy_data, partner_branding=None):
        """
        Generate a professionally formatted PDF with proper margins and text wrapping
        
        Args:
            policy_data: Dictionary containing policy content
            partner_branding: Optional partner-specific branding configuration
        
        Returns:
            BytesIO buffer containing the PDF
        """
        buffer = BytesIO()
        
        # Create document with proper margins
        doc = SimpleDocTemplate(
            buffer,
            pagesize=letter,
            rightMargin=1*inch,
            leftMargin=1*inch,
            topMargin=1*inch,
            bottomMargin=1.25*inch,
            title=policy_data.get('title', 'AI Usage Policy'),
            author='CompliGenie'
        )
        
        # Build document story
        story = []
        
        # Add title page
        story.append(Paragraph(
            policy_data.get('title', 'AI Usage Policy'),
            self.styles['CustomTitle']
        ))
        
        story.append(Spacer(1, 0.5*inch))
        
        # Add company information
        if policy_data.get('company_name'):
            story.append(Paragraph(
                f"<b>Prepared for:</b> {policy_data['company_name']}",
                self.styles['BodyTextJustified']
            ))
        
        story.append(Paragraph(
            f"<b>Effective Date:</b> {datetime.now().strftime('%B %d, %Y')}",
            self.styles['BodyTextJustified']
        ))
        
        story.append(Spacer(1, 0.5*inch))
        
        # Add table of contents placeholder
        story.append(Paragraph("Table of Contents", self.styles['SectionHeading']))
        story.append(Spacer(1, 0.25*inch))
        
        # Generate TOC entries
        toc_entries = []
        for i, section in enumerate(policy_data.get('sections', []), 1):
            toc_entries.append(
                Paragraph(f"{i}. {section['title']}", self.styles['BodyTextJustified'])
            )
        
        story.extend(toc_entries)
        story.append(PageBreak())
        
        # Add policy sections with proper formatting
        for i, section in enumerate(policy_data.get('sections', []), 1):
            # Section heading
            story.append(KeepTogether([
                Paragraph(f"{i}. {section['title']}", self.styles['SectionHeading']),
                Spacer(1, 0.1*inch)
            ]))
            
            # Section content with proper text wrapping
            content = self._wrap_text(section.get('content', ''))
            
            # Split content into paragraphs
            paragraphs = content.split('\n\n')
            for para in paragraphs:
                if para.strip():
                    # Check if this is a list item
                    if para.strip().startswith('•') or para.strip().startswith('-'):
                        # Handle bullet points
                        items = para.split('\n')
                        for item in items:
                            if item.strip():
                                clean_item = item.strip().lstrip('•-').strip()
                                story.append(Paragraph(
                                    f"• {clean_item}",
                                    self.styles['BulletList']
                                ))
                    else:
                        # Regular paragraph
                        story.append(Paragraph(
                            para.strip(),
                            self.styles['BodyTextJustified']
                        ))
                    
                    story.append(Spacer(1, 0.1*inch))
            
            # Add subsections if they exist
            for j, subsection in enumerate(section.get('subsections', []), 1):
                story.append(KeepTogether([
                    Paragraph(
                        f"{i}.{j} {subsection['title']}",
                        self.styles['SubsectionHeading']
                    ),
                    Spacer(1, 0.05*inch)
                ]))
                
                sub_content = self._wrap_text(subsection.get('content', ''))
                sub_paragraphs = sub_content.split('\n\n')
                
                for para in sub_paragraphs:
                    if para.strip():
                        story.append(Paragraph(
                            para.strip(),
                            self.styles['BodyTextJustified']
                        ))
                        story.append(Spacer(1, 0.1*inch))
            
            # Add page break after major sections (except last)
            if i < len(policy_data.get('sections', [])):
                story.append(PageBreak())
        
        # Build PDF with page number callback
        doc.build(story, onFirstPage=self._add_page_numbers, onLaterPages=self._add_page_numbers)
        
        buffer.seek(0)
        return buffer

    def apply_partner_branding(self, doc, partner_config):
        """
        Apply partner-specific branding to the document
        
        Args:
            doc: Document template
            partner_config: Partner branding configuration
        """
        if not partner_config:
            return
        
        # Update colors based on partner branding
        if partner_config.get('primary_color'):
            primary_color = colors.HexColor(partner_config['primary_color'])
            self.styles['CustomTitle'].textColor = primary_color
            self.styles['SectionHeading'].textColor = primary_color
        
        # Update fonts if specified
        if partner_config.get('font_family'):
            for style in self.styles.values():
                if hasattr(style, 'fontName'):
                    style.fontName = partner_config['font_family']