<?xml version="1.0" encoding="UTF-8"?>
<!-- This will
	1) add human-readable ids where needed
	2) Do its best to prepare for later diffing/merging.

	TODO: A) get attributes on their own lines
				B) do indenting.
				C) put entries in a definite order
				D) put senses in a definite order

		It has been tested with these processors:
			.Net 2.0
			MSXML 4
			XALAN (required removing the &#13; 's and replace them with actual carriage-returns in the <xsl:text> elements
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output indent="yes" method="xml" />
	<xsl:template match="node()">
		<xsl:text>
 </xsl:text>
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates/>
		</xsl:copy>
	   <xsl:if test="not(following-sibling::*)">
		<xsl:text>
 </xsl:text>
	   </xsl:if>
	</xsl:template>

	<xsl:template match="text()">
			<xsl:value-of select="normalize-space(.)"/>
	</xsl:template>
	<xsl:template match="@*">
		<xsl:copy>
			<xsl:apply-templates/>
		</xsl:copy>
	</xsl:template>
	<xsl:template match="entry[not(@id)]">
		<xsl:text>
</xsl:text>
		<xsl:copy>
			 <xsl:attribute name="id"><xsl:value-of select="./lexical-unit/form[1]/text"/>_<xsl:value-of select="generate-id()"/></xsl:attribute>
			 <xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>
