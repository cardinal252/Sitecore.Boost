<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!-- output directives -->
  <xsl:output method="html" indent="no" encoding="UTF-8"  />

  <!-- entry point -->
  <xsl:template match="*">
    <xsl:variable name="total" select="sum(/trace/*/@elapsed)"/>
    <xsl:variable name="processingtotal" select="sum(/trace/*[local-name()!='debuginfo' and message!='Profile rendered to output.' and message!='Profile saved.']/@elapsed)"/>
    <xsl:variable name="debugtotal" select="sum(/trace/*[local-name()='debuginfo']/@elapsed)"/>

        <xsl:apply-templates select="/trace/*" mode="line" />
  </xsl:template>

  <xsl:template match="*" mode="line">

            <xsl:value-of select="message" disable-output-escaping="yes"/>

  </xsl:template>

</xsl:stylesheet>

