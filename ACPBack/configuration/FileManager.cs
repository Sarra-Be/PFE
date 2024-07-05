using System.Collections.Generic;
using System.IO;

public class FileManager
{
	// Méthode pour obtenir les chemins des fichiers XML présents dans un répertoire spécifié
	public List<string> GetXmlFilePaths(string directoryPath)
	{
		// Initialise une liste pour stocker les chemins des fichiers XML
		List<string> xmlFilePaths = new List<string>();

		// Vérifie si le répertoire spécifié existe
		if (Directory.Exists(directoryPath))
		{
			// Obtient les chemins des fichiers XML dans le répertoire spécifié
			string[] files = Directory.GetFiles(directoryPath, "*.xml");

			// Ajoute les chemins des fichiers XML à la liste
			xmlFilePaths.AddRange(files);
		}

		// Retourne la liste des chemins des fichiers XML
		return xmlFilePaths;
	}
}