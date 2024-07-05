using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.IO;
using System.Xml;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting.Internal;

[Route("api/[controller]")]
[ApiController]
public class ExcelToXmlController : ControllerBase
{

    readonly IWebHostEnvironment webHostEnvironment;

    public ExcelToXmlController(IWebHostEnvironment webHostEnvironment)
    {
        this.webHostEnvironment = webHostEnvironment;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public IActionResult UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Fichier non valide");
        }

        try
        {
            // Load the Excel file into a memory stream
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);

                // Load the Excel package from the memory stream
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    // Create an XML document
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlNode rootNode = xmlDoc.CreateElement("Data");
                    xmlDoc.AppendChild(rootNode);

                    // Populate the XML document with data from the Excel file
                    for (int row = 1; row <= rowCount; row++)
                    {
                        XmlNode rowNode = xmlDoc.CreateElement("Row");
                        for (int col = 1; col <= colCount; col++)
                        {
                            XmlNode cellNode = xmlDoc.CreateElement("Cell");
                            cellNode.InnerText = worksheet.Cells[row, col].Value?.ToString() ?? "";
                            rowNode.AppendChild(cellNode);
                        }
                        rootNode.AppendChild(rowNode);
                    }

                    // Construct the file path for the XML file under wwwroot folder
                    var webRootPath = webHostEnvironment.WebRootPath;
                    var fileName = Path.GetFileNameWithoutExtension(file.FileName); // Get the name of the uploaded file without extension
                    var xmlFilePath = Path.Combine(webRootPath, $"{fileName}.xml"); // Construct XML file path
                    var response = new { FileName = fileName };
                    // Save the XML document to the specified file path
                    xmlDoc.Save(xmlFilePath);

                    // Return the XML file path to the client
                    return Ok(response);
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while converting the Excel file to XML: {ex.Message}");
        }
    }




    [HttpGet]
    [Route("getDataFromXml")]
    public IActionResult getDataFromXml(string file)
    {
        // Assuming 'file' parameter is for specifying the filename in the URL
        string url = $"http://localhost:5127/{file}.xml";

        try
        {
            // Download XML content from the URL
            string xmlContent;
            using (var client = new WebClient())
            {
                xmlContent = client.DownloadString(url);
            }

            // Load the downloaded XML content into an XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            // Select all <Row> elements
            XmlNodeList rowNodes = xmlDoc.SelectNodes("//Row");

            // Extract column names from the first row
            XmlNode firstRowNode = rowNodes[0];
            XmlNodeList columnNodes = firstRowNode.SelectNodes("Cell");
            List<string> columnNames = new List<string>();
            foreach (XmlNode columnNode in columnNodes)
            {
                columnNames.Add(columnNode.InnerText);
            }

            // Create a list to store row data
            List<Dictionary<string, string>> rowDataList = new List<Dictionary<string, string>>();

            // Process remaining rows
            for (int i = 1; i < rowNodes.Count; i++)
            {
                XmlNode rowNode = rowNodes[i];
                XmlNodeList cellNodes = rowNode.SelectNodes("Cell");

                // Create a dictionary to store cell data for the current row
                Dictionary<string, string> rowData = new Dictionary<string, string>();

                // Populate the dictionary with cell data
                for (int j = 0; j < columnNames.Count && j < cellNodes.Count; j++)
                {
                    rowData.Add(columnNames[j], cellNodes[j].InnerText);
                }

                // Add the row data dictionary to the list
                rowDataList.Add(rowData);
            }

            // Return the row data list as JSON
            return Ok(rowDataList);
        }
        catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
        {
            // Handle 404 error (File not found)
            return NotFound("File not found");
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

}









