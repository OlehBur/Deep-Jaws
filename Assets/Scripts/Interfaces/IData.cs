using System.IO;


public interface IData {
    //public const int INVALID_VAL = -1;
    string SavePath { get; }
    public bool IsDataExist() => File.Exists(SavePath);
    public void DeleteData() {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}
