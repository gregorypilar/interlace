<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:output encoding="utf-8" />
  
  <!-- Specified the result kind; either "document" or "fragment". -->
  <xsl:param name="result-kind" />
  
  <xsl:template name="common-styles">
    <!-- These styles may be included in documents with existing stylesheets to support
         inserting fragments. As such, all must be class specific with a class
         name prefixed by "adorned-" to avoid affecting the hosting document
         and to avoid any namespace collisions. -->
    
    span.adorned-light { color: silver; font-size: 7pt; }
    
    b.adorned-code
    {
      color: #600000;
    }
    
    div.adorned-reference-exception
    {
      color: white;
      background-color: red;
      padding: 5px;
      margin: 5px;
      margin-bottom: 1em;
    }
    
    div.adorned-error-block
    {
      color: white;
      background-color: red;
      padding: 5px;
      margin: 5px;
      margin-bottom: 1em;
    }
    
    div.adorned-verbatim
    {
      border: 1px solid silver;
      background-color: #eeeeee;
      padding: 5px;
      margin: 5px;
      margin-bottom: 1em;
    }

    pre.adorned-verbatim
    {
      margin: 0;
      padding: 0;
    }

    table.adorned
    {
      border-top: 2px solid silver;
      border-bottom: 2px solid silver;
      border-collapse: collapse;
      margin-bottom: 1em;
    }
    
    tr.adorned-heading { font-weight: bold; }

    td.adorned-body
    {
      border-top: 1px solid silver;
      border-bottom: 1px solid silver;
      padding-top: 3px;
      padding-bottom: 3px;
      padding-left: 10px;
      padding-right: 10px;
      font-family: Tahoma; font-size: 10pt;
      vertical-align: top;
    }
  </xsl:template>
  
  <xsl:template match="fragment-styles">
    <style>
      <xsl:call-template name="common-styles" />
    </style>
  </xsl:template>

  <xsl:template match="document">
    <xsl:if test="$result-kind = 'document'">
    <html>
      <head>
        <title>Document</title>
        <style>
          <!-- These styles are the default for a document: -->
          body { font-family: Tahoma; font-size: 10pt; margin: 7%; }
          h1 { font-family: Palatino Linotype; font-size: 36pt; font-weight: bold; color: #800000 }
          h2 { font-family: Palatino Linotype; font-size: 16pt; font-weight: bold; }
          h3 { font-family: Palatino Linotype; font-size: 14pt; font-weight: normal; }
          
          <!-- These styles are specific to adorned elements: -->
          <xsl:call-template name="common-styles" />
        </style>
      </head>
      <body>
        <xsl:apply-templates />
      </body>
    </html>
    </xsl:if>
    
    <xsl:if test="$result-kind = 'fragment'">
      <div class="adorned-fragment">
        <xsl:apply-templates />
      </div>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="section[parent::section[parent::document]]">
    <h1><xsl:value-of select="@title" /></h1>
    <xsl:apply-templates />
  </xsl:template>
  
  <xsl:template match="section[parent::section[parent::section[parent::section[parent::document]]]]">
    <h3><xsl:value-of select="@title" /></h3>
    <xsl:apply-templates />
  </xsl:template>
  
  <xsl:template match="section[parent::section[parent::section[parent::document]]]">
    <h2><xsl:value-of select="@title" /></h2>
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="paragraph">
    <p><xsl:apply-templates /></p>
  </xsl:template>
  
  <xsl:template match="reference[@kind='link' and starts-with(@to, 'relative:')]">
    <a><xsl:attribute name="href"><xsl:value-of select="substring-after(@to, 'relative:')" /></xsl:attribute><xsl:apply-templates /></a>
  </xsl:template>
  
  <xsl:template match="reference[@kind='link' and not(starts-with(@to, 'relative:'))]">
    <a><xsl:attribute name="href"><xsl:value-of select="@to" /></xsl:attribute><xsl:apply-templates /></a>
  </xsl:template>
  
  <xsl:template match="reference[@kind='image']">
    <img><xsl:attribute name="src"><xsl:value-of select="@to" /></xsl:attribute></img>
  </xsl:template>
  
  <xsl:template match="reference-exception">
    <div class="adorned-reference-exception">
      <b>The reference to "<xsl:value-of select="@to" />" could not be resolved due to the following error:</b><br/>
      <xsl:value-of select="@message" />
      <xsl:apply-templates />
    </div>
  </xsl:template>
  
  <xsl:template match="error-block">
    <div class="adorned-error-block">
      <xsl:apply-templates />
    </div>
  </xsl:template>
  
  <xsl:template match="message-line">
    <b><xsl:apply-templates /></b><br />
  </xsl:template>
  
  <xsl:template match="console-output">
    <div class="adorned-reference-exception">
      <xsl:apply-templates />
    </div>
  </xsl:template>
  
  <xsl:template match="console-output-line">
    <br /><xsl:value-of select="." />
  </xsl:template>
  
  <xsl:template match="q">
    "<xsl:apply-templates />"
  </xsl:template>

  <xsl:template match="list">
    <ul><xsl:apply-templates /></ul>
  </xsl:template>

  <xsl:template match="list-item">
    <li><xsl:apply-templates /></li>
  </xsl:template>
  
  <xsl:template match="verbatim-block">
    <div class="adorned-verbatim"><pre class="adorned-verbatim"><xsl:value-of select="." /></pre></div>
  </xsl:template>

  <xsl:template match="table">
    <table class="adorned">
      <xsl:apply-templates />
    </table>
  </xsl:template>
  
  <xsl:template match="table-row">
    <tr class="adorned-heading">
      <xsl:apply-templates />
    </tr>
  </xsl:template>
  
  <xsl:template match="table-row">
    <tr class="adorned-body">
      <xsl:apply-templates />
    </tr>
  </xsl:template>
  
  <xsl:template match="table-cell">
    <td class="adorned-body"><xsl:apply-templates /></td>
  </xsl:template>

  <xsl:template match="span[@kind='bold']">
    <b><xsl:apply-templates /></b>
  </xsl:template>
  
  <xsl:template match="span[@kind='italic']">
    <i><xsl:apply-templates /></i>
  </xsl:template>
  
  <xsl:template match="span[@kind='code']">
    <b class="adorned-code"><xsl:apply-templates /></b>
  </xsl:template>
  
  <xsl:template match="span[@kind='underline']">
    <u><xsl:apply-templates /></u>
  </xsl:template>
</xsl:stylesheet>

