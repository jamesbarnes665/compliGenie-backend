# app/services/pdf_generator.py

import os
from datetime import datetime
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import inch
from reportlab.platypus import (
    SimpleDocTemplate, 
    Paragraph, 
    Spacer, 
    PageBreak,
    Table,
    TableStyle,
    KeepTogether
)
from reportlab.lib.enums import TA_JUSTIFY, TA_CENTER
from reportlab.lib import colors
from io import BytesIO
import textwrap
from typing import Dict, Optional

class EnhancedPDFGenerator:
    """Enhanced PDF generator with proper formatting"""
    
    def __init__(self):
        self.styles = getSampleStyleSheet()
        self._create_custom_styles()
        
    def _create_custom_styles(self):
        """Create custom styles for the document"""
        # Title style
        self.styles.add(ParagraphStyle(
            name='CustomTitle',
            parent=self.styles['Title'],
            fontSize=24,
            textColor=colors.HexColor('#2c3e50'),
            spaceAfter=30,
            alignment=TA_CENTER
        ))
        
        # Section heading style
        self.styles.add(ParagraphStyle(
            name='SectionHeading',
            parent=self.styles['Heading1'],
            fontSize=16,
            textColor=colors.HexColor('#34495e'),
            spaceBefore=24,
            spaceAfter=12
        ))
        
        # Body text style
        self.styles.add(ParagraphStyle(
            name='BodyTextJustified',
            parent=self.styles['BodyText'],
            fontSize=11,
            alignment=TA_JUSTIFY,
            spaceBefore=6,
            spaceAfter=6,
            leading=16
        ))

    def generate_policy_pdf(self, policy_data: Dict, partner_branding: Optional[Dict] = None) -> BytesIO:
        """Generate PDF from policy content"""
        buffer = BytesIO()
        
        # Create document
        doc = SimpleDocTemplate(
            buffer,
            pagesize=letter,
            rightMargin=1*inch,
            leftMargin=1*inch,
            topMargin=1*inch,
            bottomMargin=1*inch
        )
        
        story = []
        
        # Apply partner branding if available
        primary_color = '#2c3e50'
        if partner_branding and partner_branding.get('primaryColor'):
            primary_color = partner_branding['primaryColor']
            self.styles['CustomTitle'].textColor = colors.HexColor(primary_color)
            self.styles['SectionHeading'].textColor = colors.HexColor(primary_color)
        
        # Title
        story.append(Paragraph(
            policy_data.get('title', 'AI Usage Policy'),
            self.styles['CustomTitle']
        ))
        story.append(Spacer(1, 0.5*inch))
        
        # Company info
        story.append(Paragraph(
            f"<b>Company:</b> {policy_data.get('company_name', 'N/A')}",
            self.styles['BodyTextJustified']
        ))
        story.append(Paragraph(
            f"<b>Industry:</b> {policy_data.get('industry', 'N/A')}",
            self.styles['BodyTextJustified']
        ))
        story.append(Paragraph(
            f"<b>State:</b> {policy_data.get('state', 'N/A')}",
            self.styles['BodyTextJustified']
        ))
        story.append(Paragraph(
            f"<b>Effective Date:</b> {datetime.now().strftime('%B %d, %Y')}",
            self.styles['BodyTextJustified']
        ))
        
        story.append(PageBreak())
        
        # Sections
        sections = policy_data.get('sections', [])
        for i, section in enumerate(sections, 1):
            # Section heading
            story.append(Paragraph(
                f"{i}. {section.get('title', 'Section')}",
                self.styles['SectionHeading']
            ))
            
            # Section content
            content = section.get('content', '')
            if content:
                # Wrap text properly
                wrapped_content = textwrap.fill(content, width=80)
                story.append(Paragraph(
                    wrapped_content,
                    self.styles['BodyTextJustified']
                ))
            
            story.append(Spacer(1, 0.25*inch))
            
            # Subsections
            for j, subsection in enumerate(section.get('subsections', []), 1):
                story.append(Paragraph(
                    f"{i}.{j} {subsection.get('title', 'Subsection')}",
                    self.styles['BodyTextJustified']
                ))
                sub_content = subsection.get('content', '')
                if sub_content:
                    wrapped_sub = textwrap.fill(sub_content, width=80)
                    story.append(Paragraph(
                        wrapped_sub,
                        self.styles['BodyTextJustified']
                    ))
                story.append(Spacer(1, 0.15*inch))
        
        # Build PDF
        doc.build(story)
        buffer.seek(0)
        return buffer