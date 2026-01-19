namespace CueCompanion
{
    public class ServerState
    {
        public int CurrentCueNumber { get; set; }

        public Show CurrentShow = new Show(
            new DateOnly(2026, 1, 1),
            new TimeOnly(12, 0, 0),
            new TimeOnly(14, 0, 0),
            "Default Show",
            ShowLocation.MPC,
            [
                new Cue(1, "House Lights Up", "Fade house lights to full for audience seating."),
                new Cue(2, "Intro Music", "Play opening theme track at -6dB, 30s lead-in."),
                new Cue(3, "Stage Left Spotlight", "Bring up spotlight on stage left for narrator entrance."),
                new Cue(4, "Set Change 1", "Blackout, reposition flats and props for Scene 2."),
                new Cue(5, "Wireless Mic On", "Open wireless mic on Actor A and confirm levels."),
                new Cue(6, "SFX - Door Slam", "Trigger door slam sound effect synced with action."),
                new Cue(7, "Blackout Transition", "Full blackout for scene transition, 2s fade."),
                new Cue(8, "Intermission Music", "Play intermission loop at 0dB for duration of break."),
                new Cue(9, "Pre-Finale Build", "Raise orchestral levels and prepare lighting cues."),
                new Cue(10, "Finale & Curtain", "Strobes, full stage wash and house lights up for curtain call."),
                new Cue(11, "House Lights Up", "Fade house lights to full for audience seating."),
                new Cue(12, "Intro Music", "Play opening theme track at -6dB, 30s lead-in."),
                new Cue(13, "Stage Left Spotlight", "Bring up spotlight on stage left for narrator entrance."),
                new Cue(14, "Set Change 1", "Blackout, reposition flats and props for Scene 2."),
                new Cue(15, "Wireless Mic On", "Open wireless mic on Actor A and confirm levels."),
                new Cue(16, "SFX - Door Slam", "Trigger door slam sound effect synced with action."),
                new Cue(17, "Blackout Transition", "Full blackout for scene transition, 2s fade."),
                new Cue(18, "Intermission Music", "Play intermission loop at 0dB for duration of break."),
                new Cue(19, "Pre-Finale Build", "Raise orchestral levels and prepare lighting cues."),
                new Cue(20, "Finale & Curtain", "Strobes, full stage wash and house lights up for curtain call.")
            ]
        );
    }
}
