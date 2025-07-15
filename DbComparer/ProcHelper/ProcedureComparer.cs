namespace DbComparer.ProcHelper;

public static class ProcedureComparer
{
    public static bool AreBodiesEqual(string body1, string body2)
    {
       
        string hash1 = HashHelper.ComputeHash(body1);
        string hash2 = HashHelper.ComputeHash(body2);

        return hash1 == hash2;

    }
}
