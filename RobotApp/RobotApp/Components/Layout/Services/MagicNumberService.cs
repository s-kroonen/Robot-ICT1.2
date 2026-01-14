using System.Linq;

public class MagicNumberService
{
    public int GetMagicNumber(int amount){
        return Random.Shared.Next(1, 200);

    }
}