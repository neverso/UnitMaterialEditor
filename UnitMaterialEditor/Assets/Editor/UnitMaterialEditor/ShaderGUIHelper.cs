﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArtistKit {

    public static class ShaderGUIHelper {

        public static bool Compare( String op, int a, int b ) {
            switch ( op ) {
            case "==":
                return a == b;
            case "!=":
                return a != b;
            case "<":
                return a < b;
            case "<=":
                return a <= b;
            case ">":
                return a > b;
            case ">=":
                return a >= b;
            case "&&":
                return a == 1 && b == 1;
            case "||":
                return a == 1 || b == 1;
            }
            return false;
        }

        public static bool Compare( String op, float a, float b ) {
            switch ( op ) {
            case "==":
                return a == b;
            case "!=":
                return a != b;
            case "<":
                return a < b;
            case "<=":
                return a <= b;
            case ">":
                return a > b;
            case ">=":
                return a >= b;
            }
            return false;
        }

        public static bool Compare( String op, Vector4 a, Vector4 b ) {
            switch ( op ) {
            case "==":
                return a == b;
            case "!=":
                return a != b;
            }
            return false;
        }

        public static bool ParseValue( UnitMaterialEditor gui, JSONObject obj, String fieldName, out int value ) {
            var v = obj.GetField( fieldName );
            if ( v != null ) {
                value = ( int )v.i;
                return true;
            }
            value = 0;
            return false;
        }

        public static bool ParseValue( UnitMaterialEditor gui, JSONObject obj, String fieldName, out float value ) {
            var v = obj.GetField( fieldName );
            if ( v != null && v.IsNumber ) {
                value = v.n;
                return true;
            }
            value = 0;
            return false;
        }

        public static bool ParseValue( UnitMaterialEditor gui, JSONObject obj, String fieldName, out Vector4 value ) {
            var v = obj.GetField( fieldName );
            if ( v != null && v.IsArray ) {
                var ret = new Vector4();
                for ( int i = 0; i < v.Count; ++i ) {
                    if ( v[ i ].IsNumber ) {
                        ret[ i ] = v[ i ].n;
                    }
                }
                value = ret;
                return true;
            }
            value = Vector4.zero;
            return false;
        }

        public static int ExcuteLogicOp( UnitMaterialEditor gui, MaterialProperty prop, JSONObject args ) {
            var _op = String.Empty;
            args.GetField( out _op, "op", "==" );
            if ( prop == null ) {
                String _id0, _id1;
                if ( args.GetField( out _id0, "arg0", String.Empty ) && args.GetField( out _id1, "arg1", String.Empty ) ) {
                    var _0 = EvalLogicOpArg( gui, _id0 );
                    var _1 = EvalLogicOpArg( gui, _id1 );
                    if ( _0 != -1 && _1 != -1 ) {
                        return Compare( _op, _0, _1 ) ? 1 : 0;
                    }
                }
                return -1;
            }
            switch ( prop.type ) {
            case MaterialProperty.PropType.Texture: {
                    var lh = prop.textureValue != null ? 1 : 0;
                    int rh;
                    if ( !ShaderGUIHelper.ParseValue( gui, args, "ref", out rh ) ) {
                        rh = 1;
                    }
                    return ShaderGUIHelper.Compare( _op, lh, rh ) ? 1 : 0;
                }
            case MaterialProperty.PropType.Vector: {
                    Vector4 rh;
                    if ( ShaderGUIHelper.ParseValue( gui, args, "ref", out rh ) ) {
                        return ShaderGUIHelper.Compare( _op, prop.vectorValue, rh ) ? 1 : 0;
                    }
                }
                break;
            case MaterialProperty.PropType.Color: {
                    Vector4 rh;
                    if ( ShaderGUIHelper.ParseValue( gui, args, "ref", out rh ) ) {
                        var c = prop.colorValue;
                        return ShaderGUIHelper.Compare( _op, new Vector4( c.r, c.g, c.b, c.r ), rh ) ? 1 : 0;
                    }
                }
                break;
            case MaterialProperty.PropType.Range:
            case MaterialProperty.PropType.Float: {
                    float rh = 0;
                    ShaderGUIHelper.ParseValue( gui, args, "ref", out rh );
                    return ShaderGUIHelper.Compare( _op, prop.floatValue, rh ) ? 1 : 0;
                }
            }
            return -1;
        }

        public static bool IsModeMatched( UnitMaterialEditor gui, JSONObject args ) {
            String mode;
            var modeMatched = true;
            if ( args != null && args.GetField( out mode, "mode", String.Empty ) ) {
                var renderMode = gui.FindPropEditor<ShaderGUI_RenderMode>();
                if ( renderMode != null && !String.IsNullOrEmpty( mode ) ) {
                    var names = mode.Split( '|' );
                    modeMatched = false;
                    for ( int i = 0; i < names.Length; ++i ) {
                        var name = names[ i ].Trim();
                        if ( name.Equals( renderMode._Mode.ToString(), StringComparison.OrdinalIgnoreCase ) ) {
                            modeMatched = true;
                            break;
                        }
                    }
                }
            }
            return modeMatched;
        }

        public static int EvalLogicOpArg( UnitMaterialEditor gui, String express ) {
            if ( !String.IsNullOrEmpty( express ) ) {
                var m = UnitMaterialEditor.Reg_LogicRef.Match( express );
                if ( m.Success && m.Groups.Count > 2 ) {
                    var rev = m.Groups[ 1 ].ToString().Trim() == "!";
                    var id = m.Groups[ 2 ].ToString().Trim();
                    if ( !String.IsNullOrEmpty( id ) ) {
                        var ui = gui.FindPropEditor<UnitMaterialEditor>( id );
                        if ( ui != null ) {
                            var b = ui.GetLogicOpResult();
                            if ( rev ) {
                                b = !b;
                            }
                            return b ? 1 : 0;
                        }
                        return 0;
                    }
                }
            }
            return -1;
        }
    }
}