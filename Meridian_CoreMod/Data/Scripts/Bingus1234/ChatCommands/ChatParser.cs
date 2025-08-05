using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRage;
using VRageMath;

namespace ChatCommandsN
{
    public static class ChatParser
    {
        public static bool TryParseInt(string[] command, out int result)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == null)
                    continue;

                if (int.TryParse(command[i], out result))
                {
                    command[i] = null;
                    return true;
                }
                break;
            }

            result = 0;
            return false;
        }

        public static bool TryParseFloat(string[] command, out float result)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == null)
                    continue;

                if (float.TryParse(command[i], out result))
                {
                    command[i] = null;
                    return true;
                }
                break;
            }

            result = 0;
            return false;
        }

        public static bool TryParseVector3(string[] command, out Vector3 result)
        {
            result = new Vector3();
            for (int i = 0; i < command.Length - 2; i++)
            {
                if (command[i] == null)
                    continue;

                float val;
                if (float.TryParse(command[i], out val))
                {
                    result.X = val;
                    if (float.TryParse(command[i + 1], out val))
                    {
                        result.Y = val;
                        if (float.TryParse(command[i + 2], out val))
                        {
                            result.Z = val;
                            command[i] = null;
                            command[i + 1] = null;
                            command[i + 2] = null;
                            return true;
                        }
                        break;
                    }
                    break;
                }
                break;
            }
            return false;
        }

        public static bool TryParseVector3I(string[] command, out Vector3I result)
        {
            result = new Vector3I();
            for (int i = 0; i < command.Length - 2; i++)
            {
                if (command[i] == null)
                    continue;

                int val;
                if (int.TryParse(command[i], out val))
                {
                    result.X = val;
                    if (int.TryParse(command[i + 1], out val))
                    {
                        result.Y = val;
                        if (int.TryParse(command[i + 2], out val))
                        {
                            result.Z = val;
                            command[i] = null;
                            command[i + 1] = null;
                            command[i + 2] = null;
                            return true;
                        }
                        break;
                    }
                    break;
                }
                break;
            }
            return false;
        }

        public static bool TryParseString(string[] command, out string result)
        {
            result = null;
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == null)
                    continue;

                result = command[i];
                command[i] = null;
                return true;
            }
            return false;
        }


        public static bool TryParseNamedBool(string[] command, out bool result, string falseName = "false", string trueName = "true", StringComparison comparer = StringComparison.OrdinalIgnoreCase)
        {
            result = false;
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == null)
                    continue;

                if (command[i].Equals(falseName, comparer))
                {
                    return true;
                }
                else if (command[i].Equals(trueName, comparer))
                {
                    result = true;
                    return true;
                }
                return false;
            }
            return false;
        }


        public static int ParseIntWithDefault(string[] command, int def)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == null)
                    continue;
                int res;
                if (int.TryParse(command[i], out res))
                {
                    command[i] = null;
                    return res;
                }
                break;
            }

            return def;
        }

        public static float ParseFloatWithDefault(string[] command, float def)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == null)
                    continue;
                float res;
                if (float.TryParse(command[i], out res))
                {
                    command[i] = null;
                    return res;
                }
                break;
            }

            return def;
        }

        public static Vector3 ParseVector3WithDefault(string[] command, Vector3 def)
        {
            Vector3 result = new Vector3();
            for (int i = 0; i < command.Length - 2; i++)
            {
                if (command[i] == null)
                    continue;

                float val;
                if (float.TryParse(command[i], out val))
                {
                    result.X = val;
                    if (float.TryParse(command[i + 1], out val))
                    {
                        result.Y = val;
                        if (float.TryParse(command[i + 2], out val))
                        {
                            result.Z = val;
                            command[i] = null;
                            command[i + 1] = null;
                            command[i + 2] = null;
                            return result;
                        }
                        break;
                    }
                    break;
                }
                break;
            }
            return def;
        }

        public static Vector3I ParseVector3IWithDefault(string[] command, Vector3I def)
        {
            Vector3I result = new Vector3I();
            for (int i = 0; i < command.Length - 2; i++)
            {
                if (command[i] == null)
                    continue;

                int val;
                if (int.TryParse(command[i], out val))
                {
                    result.X = val;
                    if (int.TryParse(command[i + 1], out val))
                    {
                        result.Y = val;
                        if (int.TryParse(command[i + 2], out val))
                        {
                            result.Z = val;
                            command[i] = null;
                            command[i + 1] = null;
                            command[i + 2] = null;
                            return result;
                        }
                        break;
                    }
                    break;
                }
                break;
            }
            return def;
        }

        public static string ParseStringWithDefault(string[] command, string def)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == null)
                    continue;

                string s = command[i];
                command[i] = null;
                return s;
            }
            return def;
        }

        public static bool ParseNamedBoolWithDefault(string[] command, bool def, string falseName = "false", string trueName = "true", StringComparison comparer = StringComparison.OrdinalIgnoreCase)
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == null)
                    continue;
                
                if (command[i].Equals(falseName, comparer))
                {
                    command[i] = null;
                    return false;
                }
                else if (command[i].Equals(trueName, comparer))
                {
                    command[i] = null;
                    return true;
                }
                return def;
            }
            return def;
        }
    }
}
