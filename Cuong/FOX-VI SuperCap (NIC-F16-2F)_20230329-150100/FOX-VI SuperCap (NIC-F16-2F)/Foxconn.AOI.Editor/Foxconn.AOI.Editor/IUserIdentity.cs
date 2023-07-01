namespace Foxconn.AOI.Editor
{
    public interface IUserIdentity
    {
        string LoginName { get; }

        string OperatorName { get; }

        string Shift { get; }

        bool IsInRole(string role);

        bool IsProgrammer { get; }

        bool IsOperator { get; }

        bool IsExplicitLogin { get; }
    }
}
