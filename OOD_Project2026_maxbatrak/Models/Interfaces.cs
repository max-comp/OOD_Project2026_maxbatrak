namespace OOD_Project2026_maxbatrak.Models
{
    // Implemented by any model that can be matched against a user's search query.
    // Used to provide a consistent way to filter lists across all tabs.
    public interface ISearchable
    {
        bool MatchesSearch(string query);
    }

    //Implemented by any model that can be deleted by the user.
    // Provides a confirmation message so the UI can prompt before removing.

    public interface IDeletable
    {
        string DeleteConfirmationMessage { get; }
    }
}
