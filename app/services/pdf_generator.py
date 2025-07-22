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
        
        # AI Compliance Section heading style (new)
        self.styles.add(ParagraphStyle(
            name='AIComplianceHeading',
            parent=self.styles['Heading1'],
            fontSize=16,
            textColor=colors.HexColor('#2980b9'),  # Blue color for AI compliance
            spaceBefore=24,
            spaceAfter=12,
            leftIndent=0,
            keepWithNext=True,
            leading=20,
            borderColor=colors.HexColor('#2980b9'),
            borderWidth=1,
            borderPadding=6
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
        
        # AI Compliance body text style (new)
        self.styles.add(ParagraphStyle(
            name='AIComplianceBody',
            parent=self.styles['BodyText'],
            fontSize=11,
            alignment=TA_JUSTIFY,
            spaceBefore=6,
            spaceAfter=6,
            leftIndent=0,
            rightIndent=0,
            leading=16,
            textColor=colors.HexColor('#2c3e50'),
            backColor=colors.HexColor('#ecf0f1')  # Light gray background
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
        
        # AI Compliance Notice style (new)
        self.styles.add(ParagraphStyle(
            name='ComplianceNotice',
            parent=self.styles['BodyText'],
            fontSize=10,
            alignment=TA_CENTER,
            spaceBefore=12,
            spaceAfter=12,
            textColor=colors.HexColor('#2980b9'),
            borderColor=colors.HexColor('#2980b9'),
            borderWidth=1,
            borderPadding=8,
            leading=14
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

    def _is_ai_compliance_section(self, section_title):
        """Check if this is one of the new AI compliance sections"""
        ai_sections = [
            "AI Transparency Requirements",
            "AI Bias Prevention Measures", 
            "AI Audit Trail Requirements"
        ]
        return any(ai_section in section_title for ai_section in ai_sections)

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
        
        # Add compliance notice for policies with AI compliance sections
        has_ai_compliance = any(self._is_ai_compliance_section(section.get('title', '')) 
                               for section in policy_data.get('sections', []))
        
        if has_ai_compliance:
            story.append(Paragraph(
                "<b>This policy includes enhanced AI compliance sections for transparency, "
                "bias prevention, and audit trail requirements.</b>",
                self.styles['ComplianceNotice']
            ))
            story.append(Spacer(1, 0.25*inch))
        
        # Add table of contents placeholder
        story.append(Paragraph("Table of Contents", self.styles['SectionHeading']))
        story.append(Spacer(1, 0.25*inch))
        
        # Generate TOC entries with special highlighting for AI compliance
        toc_entries = []
        for i, section in enumerate(policy_data.get('sections', []), 1):
            section_title = section['title']
            if self._is_ai_compliance_section(section_title):
                # Highlight AI compliance sections in TOC
                toc_entries.append(
                    Paragraph(
                        f"<font color='#2980b9'><b>{i}. {section_title} [AI Compliance]</b></font>",
                        self.styles['BodyTextJustified']
                    )
                )
            else:
                toc_entries.append(
                    Paragraph(f"{i}. {section_title}", self.styles['BodyTextJustified'])
                )
        
        story.extend(toc_entries)
        story.append(PageBreak())
        
        # Add policy sections with proper formatting
        for i, section in enumerate(policy_data.get('sections', []), 1):
            section_title = section['title']
            is_ai_compliance = self._is_ai_compliance_section(section_title)
            
            # Add special notice before AI compliance sections
            if is_ai_compliance:
                story.append(Paragraph(
                    "<b>AI COMPLIANCE SECTION</b>",
                    self.styles['ComplianceNotice']
                ))
                story.append(Spacer(1, 0.1*inch))
            
            # Section heading with appropriate style
            heading_style = 'AIComplianceHeading' if is_ai_compliance else 'SectionHeading'
            story.append(KeepTogether([
                Paragraph(f"{i}. {section_title}", self.styles[heading_style]),
                Spacer(1, 0.1*inch)
            ]))
            
            # Section content with proper text wrapping
            content = self._wrap_text(section.get('content', ''))
            body_style = 'AIComplianceBody' if is_ai_compliance else 'BodyTextJustified'
            
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
                            self.styles[body_style]
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
                        # Check for numbered lists (e.g., "(1) Item")
                        if para.strip().startswith('(') and ')' in para[:4]:
                            # Handle numbered lists
                            items = para.split(';')
                            for item in items:
                                if item.strip():
                                    story.append(Paragraph(
                                        item.strip(),
                                        self.styles['BulletList']
                                    ))
                        else:
                            story.append(Paragraph(
                                para.strip(),
                                self.styles[body_style] if is_ai_compliance else self.styles['BodyTextJustified']
                            ))
                        story.append(Spacer(1, 0.1*inch))
            
            # Add visual separator after AI compliance sections
            if is_ai_compliance:
                story.append(Spacer(1, 0.2*inch))
                # Add a subtle line separator
                separator_data = [['']]
                separator_table = Table(separator_data, colWidths=[6.5*inch])
                separator_table.setStyle(TableStyle([
                    ('LINEABOVE', (0, 0), (-1, 0), 1, colors.HexColor('#2980b9')),
                ]))
                story.append(separator_table)
                story.append(Spacer(1, 0.2*inch))
            
            # Add page break after major sections (except last)
            if i < len(policy_data.get('sections', [])):
                story.append(PageBreak())
        
        # Add compliance summary at the end if AI compliance sections exist
        if has_ai_compliance:
            story.append(PageBreak())
            story.append(Paragraph("AI Compliance Summary", self.styles['SectionHeading']))
            story.append(Spacer(1, 0.25*inch))
            
            summary_text = """This policy incorporates comprehensive AI compliance measures including:
            
            <b>Transparency Requirements:</b> Clear documentation and disclosure of AI usage in all decision-making processes affecting stakeholders.
            
            <b>Bias Prevention:</b> Systematic testing and monitoring to ensure fair and equitable AI outcomes across all demographic groups.
            
            <b>Audit Trail Standards:</b> Comprehensive logging and retention of all AI interactions to enable accountability and regulatory compliance.
            
            These measures ensure responsible AI deployment aligned with emerging regulations and industry best practices."""
            
            story.append(Paragraph(summary_text, self.styles['BodyTextJustified']))
        
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