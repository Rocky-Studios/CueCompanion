using CueCompanion.Client;

namespace CueCompanion;

public class ServerState
{
    public Connection[] Connections { get; set; } = new[]
    {
        new Connection("Sound")
    };

    public Show CurrentShow { get; set; } = new();
    public int CurrentCueNumber { get; set; }

    public Cue CurrentCue =>
        CurrentShow.Cues.FirstOrDefault(c => c.CueNumber == CurrentCueNumber) ?? new Cue();

    private static Show GetSampleShow()
    {
        Role sound = new("Sound", "Gergo");
        Role graphics = new("Graphics", "Reece");
        Role lights = new("Lights/VFX", "Chuck");
        Role aux = new("Aux/Camera", "Sheldon");
        Role stage = new("Stage", "Hyunseo");

        Show show = new(
            new DateOnly(2026, 1, 1),
            new TimeOnly(12, 0, 0),
            new TimeOnly(14, 0, 0),
            "Default Show",
            ShowLocation.MPC,
            new[]
            {
                new Role("Sound", "Gergo"),
                new Role("Graphics", "Reece"),
                new Role("Lights/VFX", "Chuck"),
                new Role("Aux/Camera", "Sheldon"),
                new Role("Stage", "Hyunseo")
            },
            new[]
            {
                new Cue(1, "House Open", "Begin pre-show sequence",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Fade pre-roll track to 70% and loop ambient bed" },
                        { "Graphics", "Display pre-show slide with sponsor logos" },
                        { "Lights/VFX", "House lights to 80% warm" },
                        { "Aux/Camera", "Standby pre-show video; camera 1 on audience" },
                        { "Stage", "Stage doors open; usher lights on wing" }
                    },
                    new Dictionary<string, string>
                    {
                        { "All", "Ensure all crew are at stations and ready" }
                    }
                ),

                new Cue(2, "Intro Stinger", "Short stinger and title reveal",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Play stinger SFX at full" },
                        { "Graphics", "Animate show title intro (0.5s fade)" },
                        { "Lights/VFX", "Quick wash on stage + gobo sweep" },
                        { "Aux/Camera", "Cut to host close-up on camera 2" },
                        { "Stage", "Host mark center stage; mic on" }
                    }
                ),

                new Cue(3, "Host Mic Up", "Live mic for host begins",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Turn up host mic + remove mute; monitor mix" },
                        { "Graphics", "Lower caption overlay" },
                        { "Lights/VFX", "Spot on host (followspot 1)" },
                        { "Aux/Camera", "Camera 2 follow host; switch to program" },
                        { "Stage", "Confirm lav placement; prop set ready" }
                    }
                ),

                new Cue(4, "Video Roll 1", "Play pre-recorded video package",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Patch video audio to LR; fade music under" },
                        { "Graphics", "Full-screen video; lower-thirds off" },
                        { "Lights/VFX", "Dim house lights to 20%; blackout edges" },
                        { "Aux/Camera", "Playback video; slate 'Roll 1' to truck" },
                        { "Stage", "Clear stage for video content" }
                    }
                ),

                new Cue(5, "SFX Thunder", "SFX cue during dramatic moment",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Trigger thunder SFX at -2dB FS" },
                        { "Graphics", "Flash lightning frame for 0.2s" },
                        { "Lights/VFX", "Quick strobe + cool blue wash" },
                        { "Aux/Camera", "Camera jump-cut to reaction shot" },
                        { "Stage", "Safety check: loose props secured" }
                    }
                ),

                new Cue(6, "Lighting Accent", "Accent lighting on guest",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Lower ambient bed to -6dB" },
                        { "Graphics", "On-screen lower-third: Guest name" },
                        { "Lights/VFX", "Highlight guest with key + rim lights" },
                        { "Aux/Camera", "Camera 3 tighten on guest" },
                        { "Stage", "Cue guest to mark B" }
                    }
                ),

                new Cue(7, "Camera Play", "Switch between cameras for montage",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Mix room mics to stereo" },
                        { "Graphics", "Prepare montage overlay" },
                        { "Lights/VFX", "Smooth crossfade between states" },
                        { "Aux/Camera", "Auto-sequence cameras: 1->2->3" },
                        { "Stage", "Move set piece to center for montage" }
                    }
                ),

                new Cue(8, "Spotlight On", "Single spotlight for monologue",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Bring up spoken word mic + compress" },
                        { "Graphics", "Remove on-screen elements" },
                        { "Lights/VFX", "Followspot 1 full intensity on performer" },
                        { "Aux/Camera", "Pin camera 2 on performer with tight framing" },
                        { "Stage", "Silent stage: no movement" }
                    }
                ),

                new Cue(9, "Dance Intro", "Music kick and choreography start",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Quick fade in dance track to 0dB" },
                        { "Graphics", "Enable dance lower-third & BPM overlay" },
                        { "Lights/VFX", "Activate moving lights sequence preset A" },
                        { "Aux/Camera", "Program camera dolly move for wide shot" },
                        { "Stage", "Trap floor up; set risers in position" }
                    }
                ),

                new Cue(10, "Intermission Music", "Lower energy music for intermission",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Crossfade to intermission playlist at -3dB" },
                        { "Graphics", "Show intermission timer (10:00) on screen" },
                        { "Lights/VFX", "House lights to 60%; aisle lights on" },
                        { "Aux/Camera", "Loop audience wide camera; record highlights" },
                        { "Stage", "Lock stage elements; check rigging" }
                    }
                ),

                new Cue(11, "Act Two Open", "Act two opening flourish",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Play act-two sting then cut to underscore" },
                        { "Graphics", "Transition graphic: 'Act Two' with dissolve" },
                        { "Lights/VFX", "Full stage wash with warm tones" },
                        { "Aux/Camera", "Camera crossfade to stage-wide view" },
                        { "Stage", "Reveal backdrop; reset props" }
                    }
                ),

                new Cue(12, "Mic Swap", "Switch mic from host to guest",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Mute host; unmute guest; check levels" },
                        { "Graphics", "Update lower-third to guest" },
                        { "Lights/VFX", "Shift spot from host to guest" },
                        { "Aux/Camera", "Cut camera to guest close-up" },
                        { "Stage", "Hand microphone to guest; confirm placement" }
                    }
                ),

                new Cue(13, "Countdown Clock", "On-screen countdown for timed bit",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Bring up countdown SFX loop quietly" },
                        { "Graphics", "Display 00:00:30 countdown overlay" },
                        { "Lights/VFX", "Accent pulse with each 10s mark" },
                        { "Aux/Camera", "Cue camera to timer graphic when 5s remain" },
                        { "Stage", "Prepare contestant for buzzer" }
                    }
                ),

                new Cue(14, "Buzzer", "End of timed segment buzzer",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Trigger buzzer SFX and duck music" },
                        { "Graphics", "Flash red border and stop countdown" },
                        { "Lights/VFX", "Quick red blink across stage" },
                        { "Aux/Camera", "Cut to reaction shots" },
                        { "Stage", "Signal stage manager for next move" }
                    }
                ),

                new Cue(15, "Set Piece Lower", "Lower set piece into view",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Quiet underscoring; safety chime at start" },
                        { "Graphics", "Hide lower-thirds during movement" },
                        { "Lights/VFX", "Follow set movement with soft wash" },
                        { "Aux/Camera", "Hold cameras steady; no moves" },
                        { "Stage", "Operate fly system: lower scenic piece slowly" }
                    }
                ),

                new Cue(16, "SFX Wind", "Wind SFX for outdoor scene",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Play wind ambience loop at -8dB" },
                        { "Graphics", "Subtle animated background wind" },
                        { "Lights/VFX", "Cool blue wash + slow backlight flicker" },
                        { "Aux/Camera", "Add subtle camera shake preset" },
                        { "Stage", "Secure loose fabrics; warn performers" }
                    }
                ),

                new Cue(17, "Live Band In", "Band begins live performance",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Open band subgroup; monitor FOH levels" },
                        { "Graphics", "Show band name and track credits" },
                        { "Lights/VFX", "Activate concert lighting preset B" },
                        { "Aux/Camera", "Stage cameras: add wide crane shot" },
                        { "Stage", "Assist band with wedge placement" }
                    }
                ),

                new Cue(18, "Blackout", "Full blackout for scene change",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Fade all audio to -inf over 1s; cue blackout SFX" },
                        { "Graphics", "Clear screens to black" },
                        { "Lights/VFX", "Blackout all stage and house lights" },
                        { "Aux/Camera", "Hold last camera frame; prepare for new scene" },
                        { "Stage", "Execute dark change; move set pieces" }
                    }
                ),

                new Cue(19, "Safety Check", "Pre-effect safety confirmation",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Standby communicators; hold audio cues" },
                        { "Graphics", "Display safety hold overlay to crew monitor" },
                        { "Lights/VFX", "Cue warning strobes at low intensity" },
                        { "Aux/Camera", "Confirm camera and cable clearances" },
                        { "Stage", "Final check: pyros off / confirmed safe" }
                    }
                ),

                new Cue(20, "Finale", "Full cast finale and blackout",
                    new Dictionary<string, string>
                    {
                        { "Sound", "Bring music to peak; submix backup tracks" },
                        { "Graphics", "Firework overlay + sponsor crawl" },
                        { "Lights/VFX", "Full stage blinder + color sweep" },
                        { "Aux/Camera", "Rapid camera cutting + crane wide shot" },
                        { "Stage", "Cast to marks; confetti cue on GO" }
                    }
                )
            }
        );
        return show;
    }
}