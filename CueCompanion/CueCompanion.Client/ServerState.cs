using CueCompanion.Client;

namespace CueCompanion
{
    public class ServerState
    {
        public int CurrentCueNumber { get; set; }

        public Show CurrentShow = GetSampleShow();

        private static Show GetSampleShow()
        {
            Role sound = new Role("Sound", "Gergo");
            Role graphics = new Role("Graphics", "Reece");
            Role lights = new Role("Lights/VFX", "Chuck");
            Role aux = new Role("Aux/Camera", "Sheldon");
            Role stage = new Role("Stage", "Hyunseo");
            
            Show show = new Show(
                new DateOnly(2026, 1, 1),
                new TimeOnly(12, 0, 0),
                new TimeOnly(14, 0, 0),
                "Default Show",
                ShowLocation.MPC,
                [
                    new Role("Sound", "Gergo"),
                    new Role("Graphics", "Reece"),
                    new Role("Lights/VFX", "Chuck"),
                    new Role("Aux/Camera", "Sheldon"),
                    new Role("Stage", "Hyunseo"),
                ],
                [
                new Cue(1, "House Open", "Begin pre-show sequence",
                    new Dictionary<Role, string>
                    {
                        { sound, "Fade pre-roll track to 70% and loop ambient bed" },
                        { graphics, "Display pre-show slide with sponsor logos" },
                        { lights, "House lights to 80% warm" },
                        { aux, "Standby pre-show video; camera 1 on audience" },
                        { stage, "Stage doors open; usher lights on wing" }
                    }
                ),

                new Cue(2, "Intro Stinger", "Short stinger and title reveal",
                    new Dictionary<Role, string>
                    {
                        { sound, "Play stinger SFX at full" },
                        { graphics, "Animate show title intro (0.5s fade)" },
                        { lights, "Quick wash on stage + gobo sweep" },
                        { aux, "Cut to host close-up on camera 2" },
                        { stage, "Host mark center stage; mic on" }
                    }
                ),

                new Cue(3, "Host Mic Up", "Live mic for host begins",
                    new Dictionary<Role, string>
                    {
                        { sound, "Turn up host mic + remove mute; monitor mix" },
                        { graphics, "Lower caption overlay" },
                        { lights, "Spot on host (followspot 1)" },
                        { aux, "Camera 2 follow host; switch to program" },
                        { stage, "Confirm lav placement; prop set ready" }
                    }
                ),

                new Cue(4, "Video Roll 1", "Play pre-recorded video package",
                    new Dictionary<Role, string>
                    {
                        { sound, "Patch video audio to LR; fade music under" },
                        { graphics, "Full-screen video; lower-thirds off" },
                        { lights, "Dim house lights to 20%; blackout edges" },
                        { aux, "Playback video; slate 'Roll 1' to truck" },
                        { stage, "Clear stage for video content" }
                    }
                ),

                new Cue(5, "SFX Thunder", "SFX cue during dramatic moment",
                    new Dictionary<Role, string>
                    {
                        { sound, "Trigger thunder SFX at -2dB FS" },
                        { graphics, "Flash lightning frame for 0.2s" },
                        { lights, "Quick strobe + cool blue wash" },
                        { aux, "Camera jump-cut to reaction shot" },
                        { stage, "Safety check: loose props secured" }
                    }
                ),

                new Cue(6, "Lighting Accent", "Accent lighting on guest",
                    new Dictionary<Role, string>
                    {
                        { sound, "Lower ambient bed to -6dB" },
                        { graphics, "On-screen lower-third: Guest name" },
                        { lights, "Highlight guest with key + rim lights" },
                        { aux, "Camera 3 tighten on guest" },
                        { stage, "Cue guest to mark B" }
                    }
                ),

                new Cue(7, "Camera Play", "Switch between cameras for montage",
                    new Dictionary<Role, string>
                    {
                        { sound, "Mix room mics to stereo" },
                        { graphics, "Prepare montage overlay" },
                        { lights, "Smooth crossfade between states" },
                        { aux, "Auto-sequence cameras: 1->2->3" },
                        { stage, "Move set piece to center for montage" }
                    }
                ),

                new Cue(8, "Spotlight On", "Single spotlight for monologue",
                    new Dictionary<Role, string>
                    {
                        { sound, "Bring up spoken word mic + compress" },
                        { graphics, "Remove on-screen elements" },
                        { lights, "Followspot 1 full intensity on performer" },
                        { aux, "Pin camera 2 on performer with tight framing" },
                        { stage, "Silent stage: no movement" }
                    }
                ),

                new Cue(9, "Dance Intro", "Music kick and choreography start",
                    new Dictionary<Role, string>
                    {
                        { sound, "Quick fade in dance track to 0dB" },
                        { graphics, "Enable dance lower-third & BPM overlay" },
                        { lights, "Activate moving lights sequence preset A" },
                        { aux, "Program camera dolly move for wide shot" },
                        { stage, "Trap floor up; set risers in position" }
                    }
                ),

                new Cue(10, "Intermission Music", "Lower energy music for intermission",
                    new Dictionary<Role, string>
                    {
                        { sound, "Crossfade to intermission playlist at -3dB" },
                        { graphics, "Show intermission timer (10:00) on screen" },
                        { lights, "House lights to 60%; aisle lights on" },
                        { aux, "Loop audience wide camera; record highlights" },
                        { stage, "Lock stage elements; check rigging" }
                    }
                ),

                new Cue(11, "Act Two Open", "Act two opening flourish",
                    new Dictionary<Role, string>
                    {
                        { sound, "Play act-two sting then cut to underscore" },
                        { graphics, "Transition graphic: 'Act Two' with dissolve" },
                        { lights, "Full stage wash with warm tones" },
                        { aux, "Camera crossfade to stage-wide view" },
                        { stage, "Reveal backdrop; reset props" }
                    }
                ),

                new Cue(12, "Mic Swap", "Switch mic from host to guest",
                    new Dictionary<Role, string>
                    {
                        { sound, "Mute host; unmute guest; check levels" },
                        { graphics, "Update lower-third to guest" },
                        { lights, "Shift spot from host to guest" },
                        { aux, "Cut camera to guest close-up" },
                        { stage, "Hand microphone to guest; confirm placement" }
                    }
                ),

                new Cue(13, "Countdown Clock", "On-screen countdown for timed bit",
                    new Dictionary<Role, string>
                    {
                        { sound, "Bring up countdown SFX loop quietly" },
                        { graphics, "Display 00:00:30 countdown overlay" },
                        { lights, "Accent pulse with each 10s mark" },
                        { aux, "Cue camera to timer graphic when 5s remain" },
                        { stage, "Prepare contestant for buzzer" }
                    }
                ),

                new Cue(14, "Buzzer", "End of timed segment buzzer",
                    new Dictionary<Role, string>
                    {
                        { sound, "Trigger buzzer SFX and duck music" },
                        { graphics, "Flash red border and stop countdown" },
                        { lights, "Quick red blink across stage" },
                        { aux, "Cut to reaction shots" },
                        { stage, "Signal stage manager for next move" }
                    }
                ),

                new Cue(15, "Set Piece Lower", "Lower set piece into view",
                    new Dictionary<Role, string>
                    {
                        { sound, "Quiet underscoring; safety chime at start" },
                        { graphics, "Hide lower-thirds during movement" },
                        { lights, "Follow set movement with soft wash" },
                        { aux, "Hold cameras steady; no moves" },
                        { stage, "Operate fly system: lower scenic piece slowly" }
                    }
                ),

                new Cue(16, "SFX Wind", "Wind SFX for outdoor scene",
                    new Dictionary<Role, string>
                    {
                        { sound, "Play wind ambience loop at -8dB" },
                        { graphics, "Subtle animated background wind" },
                        { lights, "Cool blue wash + slow backlight flicker" },
                        { aux, "Add subtle camera shake preset" },
                        { stage, "Secure loose fabrics; warn performers" }
                    }
                ),

                new Cue(17, "Live Band In", "Band begins live performance",
                    new Dictionary<Role, string>
                    {
                        { sound, "Open band subgroup; monitor FOH levels" },
                        { graphics, "Show band name and track credits" },
                        { lights, "Activate concert lighting preset B" },
                        { aux, "Stage cameras: add wide crane shot" },
                        { stage, "Assist band with wedge placement" }
                    }
                ),

                new Cue(18, "Blackout", "Full blackout for scene change",
                    new Dictionary<Role, string>
                    {
                        { sound, "Fade all audio to -inf over 1s; cue blackout SFX" },
                        { graphics, "Clear screens to black" },
                        { lights, "Blackout all stage and house lights" },
                        { aux, "Hold last camera frame; prepare for new scene" },
                        { stage, "Execute dark change; move set pieces" }
                    }
                ),

                new Cue(19, "Safety Check", "Pre-effect safety confirmation",
                    new Dictionary<Role, string>
                    {
                        { sound, "Standby communicators; hold audio cues" },
                        { graphics, "Display safety hold overlay to crew monitor" },
                        { lights, "Cue warning strobes at low intensity" },
                        { aux, "Confirm camera and cable clearances" },
                        { stage, "Final check: pyros off / confirmed safe" }
                    }
                ),

                new Cue(20, "Finale", "Full cast finale and blackout",
                    new Dictionary<Role, string>
                    {
                        { sound, "Bring music to peak; submix backup tracks" },
                        { graphics, "Firework overlay + sponsor crawl" },
                        { lights, "Full stage blinder + color sweep" },
                        { aux, "Rapid camera cutting + crane wide shot" },
                        { stage, "Cast to marks; confetti cue on GO" }
                    }
                )
                ]
            );
            return show;
        }
    }
}
