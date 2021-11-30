using System.Xml;

if (args.Length < 2)
{
    Console.Error.WriteLine("Error: No has especificado la ruta del archivo a modificar y/o el archivo de salida.");
    return 1;
}

XmlDocument digiTab = new XmlDocument();
digiTab.Load(args[0]);
var root = digiTab.DocumentElement;

if( root == null )
    return 1;

var nodosSeleccionados = root.SelectNodes("/digitab/codes/code");
if (nodosSeleccionados == null)
    return 1;

foreach (XmlNode nodoCódigo in nodosSeleccionados)
{
    if (nodoCódigo == null)
        continue;

    var nombreCódigo = nodoCódigo.Attributes?["name"]?.InnerText ?? string.Empty;
    var commands = nodoCódigo.SelectSingleNode("commands");
    if (commands == null)
        continue;

    var líneas = commands.InnerText.Split("\r\n");
    var atributos = líneas.Where(l => l.StartsWith("asigna_atributo"));
    var órdenes = líneas.Where(l => !l.StartsWith("asigna_atributo") && !l.StartsWith("resetea_atributos_bbdd"));

    // Creamos el nuevo nodo "commands"
    var cadenaÓrdenes = string.Empty;
    foreach(var orden in órdenes)
        if( orden.Length > 0 )
            cadenaÓrdenes += orden + "\n";

    var commandsNuevo = digiTab.CreateNode(XmlNodeType.Element, "commands", "");
    commandsNuevo.AppendChild(digiTab.CreateCDataSection(cadenaÓrdenes));
    nodoCódigo.ReplaceChild(commandsNuevo, commands);

    // Creamos el nodo "databaseAttributes"
    var databaseAttributes = digiTab.CreateNode(XmlNodeType.Element, "databaseAttributes", "");
    foreach(var atributo in atributos)
    {
        var palabras = atributo.Split('=', ' ');
        if (palabras.Length < 4)
            continue;

        var field = digiTab.CreateNode(XmlNodeType.Element, "field", "");
        if (field == null)
            continue;

        var name = digiTab.CreateAttribute("name");
        name.InnerText = palabras[2];

        var value = digiTab.CreateAttribute("value");
        value.InnerText = palabras[3].Replace("\"", String.Empty);

        field.Attributes?.Append(name);
        field.Attributes?.Append(value);
        databaseAttributes.AppendChild(field);
    }
    nodoCódigo.InsertBefore(databaseAttributes, commandsNuevo);
}

digiTab.Save(args[1]);

return 0;

