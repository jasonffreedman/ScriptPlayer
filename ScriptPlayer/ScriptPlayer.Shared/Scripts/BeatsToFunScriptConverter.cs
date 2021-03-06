﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptPlayer.Shared.Scripts
{
    public enum ConversionMode
    {
        /// <summary>
        /// One beat up, next beat down
        /// </summary>
        UpOrDown,
        UpDownFast,
        DownFast,
        DownCenter,
        UpFast,
        UpCenter
    }

    public class ConversionSettings
    {
        public ConversionMode Mode { get; set; }
        public byte Min { get; set; }
        public byte Max { get; set; }
    }

    public static class BeatsToFunScriptConverter
    {
        public static List<FunScriptAction> Convert(IEnumerable<TimeSpan> timestamps, ConversionSettings settings, int startIndex = 0)
        {
            var beats = timestamps.OrderBy(a => a).ToList();
            var actions = new List<FunScriptAction>();

            TimeSpan previousTimeStamp = TimeSpan.FromDays(-1);
            TimeSpan previousDuration = TimeSpan.FromDays(1);
            
            TimeSpan centerLimit;

            bool up = startIndex % 2 == 0;

            byte positionDown;
            byte positionUp;

            switch (settings.Mode)
            {
                case ConversionMode.UpOrDown:
                    centerLimit = TimeSpan.Zero;
                    positionDown = settings.Min;
                    positionUp = settings.Max;
                    break;
                case ConversionMode.UpDownFast:
                    centerLimit = TimeSpan.Zero;
                    positionDown = settings.Min;
                    positionUp = settings.Max;
                    break;
                case ConversionMode.DownFast:
                    centerLimit = TimeSpan.FromMilliseconds(180);
                    positionDown = settings.Max;
                    positionUp = settings.Min;
                    break;
                case ConversionMode.DownCenter:
                    centerLimit = TimeSpan.FromMilliseconds(2000);
                    positionDown = settings.Max;
                    positionUp = settings.Min;
                    break;
                case ConversionMode.UpFast:
                    centerLimit = TimeSpan.FromMilliseconds(180);
                    positionDown = settings.Min;
                    positionUp = settings.Max;
                    break;
                case ConversionMode.UpCenter:
                    centerLimit = TimeSpan.FromMilliseconds(2000);
                    positionDown = settings.Min;
                    positionUp = settings.Max;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(settings.Mode), settings.Mode, null);
            }

            for (int index = 0; index < beats.Count; index++)
            {
                TimeSpan timestamp = beats[index];
                up ^= true;

                switch (settings.Mode)
                {
                    case ConversionMode.UpDownFast:
                    {
                        if (index > 0)
                        {
                            TimeSpan duration = timestamp - previousTimeStamp;

                            if (duration > previousDuration)
                            {
                                actions.Add(new FunScriptAction
                                {
                                    Position = up ? positionUp : positionDown,
                                    TimeStamp = timestamp - (duration - previousDuration)
                                });
                            }
                        }

                        actions.Add(new FunScriptAction
                        {
                            Position = up ? positionUp : positionDown,
                            TimeStamp = timestamp
                        });
                        break;
                    }
                    case ConversionMode.UpOrDown:
                    {
                        actions.Add(new FunScriptAction
                        {
                            Position = up ? positionUp : positionDown,
                            TimeStamp = timestamp
                        });
                        break;
                    }
                    case ConversionMode.UpCenter:
                    case ConversionMode.DownCenter:
                    case ConversionMode.UpFast:
                    case ConversionMode.DownFast:
                    {
                        if (previousTimeStamp != TimeSpan.MinValue)
                        {
                            if (timestamp - previousTimeStamp >= centerLimit.Multiply(2))
                            {
                                actions.Add(new FunScriptAction
                                {
                                    Position = positionDown,
                                    TimeStamp = previousTimeStamp + centerLimit
                                });

                                actions.Add(new FunScriptAction
                                {
                                    Position = positionDown,
                                    TimeStamp = timestamp - centerLimit
                                });
                            }
                            else
                            {
                                actions.Add(new FunScriptAction
                                {
                                    Position = positionDown,
                                    TimeStamp = (previousTimeStamp + timestamp).Divide(2)
                                });
                            }
                        }
                    }

                        actions.Add(new FunScriptAction
                        {
                            Position = positionUp,
                            TimeStamp = timestamp
                        });

                        break;
                }

                previousDuration = timestamp - previousTimeStamp;

                previousTimeStamp = timestamp;
            }

            return actions;
        }
    }
}
