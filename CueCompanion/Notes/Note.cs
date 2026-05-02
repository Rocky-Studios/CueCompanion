using SQLite;

namespace CueCompanion.Notes;

[Table("notes")]
public class Note
{
    [Column("id")]
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }

    [Column("role_id")]
    public int? RoleId { get; set; }

    [Column("cue_list")]
    public byte[] CueListAsBytes { get; set; }

    public int[] CueList
    {
        get
        {
            int[] cueList                                       = new int[CueListAsBytes.Length / 4];
            for (int i = 0; i < cueList.Length; i++) cueList[i] = BitConverter.ToInt32(CueListAsBytes, i * 4);

            return cueList;
        }
    }

    [Column("visibility")]
    public NoteVisibility Visibility { get; set; }

    [Column("assignment")]
    public NoteAssignment Assignment { get; set; }

    [Column("note_text")]
    public string NoteText { get; set; }
}

public enum NoteVisibility
{
    Me               = 0,
    EveryoneWithRole = 1,
    Everyone         = 2,
}

public enum NoteAssignment
{
    SingleCue = 0,
    CueList   = 1,
    AllCues   = 2,
    AllShows  = 3,
}