namespace Models;

public interface ISerializableTool
{
    byte[] Serialize();
    void Deserialize(byte[] payload);
}
