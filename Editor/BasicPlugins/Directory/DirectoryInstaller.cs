using System.IO;

namespace CMS.Editor
{
    public class DirectoryInstaller : ExtensionInstaller<ContextMenuAction>
    {
        public override void Install(Locator locator, IRegistrator<ContextMenuAction> registrator)
        {
            registrator.Register(new ContextMenuAction("Create/Folder", fileEntry =>
                {
                    var directory = fileEntry.IsDirectory ? fileEntry.Path : Path.GetDirectoryName(fileEntry.Path);

                    if (string.IsNullOrEmpty(directory))
                    {
                        throw new System.Exception("Parent directory name is empty");
                    }
                    
                    var popups = locator.Get<PopupsWrapper>();

                    popups.ShowPopup
                    (
                        PopupDefinition.WithTitle("Create Folder")
                            .WithTextField(new TextFieldDefinition("Name"))
                            .WithButtons(new ButtonDefinition("Create", folderName =>
                            {
                                if (string.IsNullOrEmpty(folderName))
                                {
                                    throw new System.Exception("Folder name is empty");
                                }
                                
                                var path = Path.Combine(directory, folderName);
                                Directory.CreateDirectory(Path.GetFullPath(path));
                            }))
                );
            }));
        }
    }
}