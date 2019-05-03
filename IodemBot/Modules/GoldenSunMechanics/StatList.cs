﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static IodemBot.Modules.GoldenSunMechanics.Psynergy;

namespace IodemBot.Modules.GoldenSunMechanics
{
    public class StatList
    {
        private static Dictionary<string, Stats> stats;
        private static Stats baseStats = new Stats(35, 20, 20, 6, 8); //30, 20, 11, 6, 8

        static StatList()
        {
            string json = File.ReadAllText("Resources/stats.json");
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            stats = data.ToObject<Dictionary<string, Stats>>();
        }
    }

    public struct ElementalStats
    {
        public int VenusAtk { get; set; }
        public int VenusRes { get; set; }
        public int MarsAtk { get; set; }
        public int MarsRes { get; set; }
        public int JupiterAtk { get; set; }
        public int JupiterRes { get; set; }
        public int MercuryAtk { get; set; }
        public int MercuryRes { get; set; }

        public ElementalStats(int venusAtk, int venusRes, int marsAtk, int marsDef, int jupiterAtk, int jupiterDef, int mercuryAtk, int mercuryDef) : this()
        {
            VenusAtk = venusAtk;
            VenusRes = venusRes;
            MarsAtk = marsAtk;
            MarsRes = marsDef;
            JupiterAtk = jupiterAtk;
            JupiterRes = jupiterDef;
            MercuryAtk = mercuryAtk;
            MercuryRes = mercuryDef;
        }

        public static ElementalStats operator +(ElementalStats s1, ElementalStats s2)
        {
            return new ElementalStats(s1.VenusAtk + s2.VenusAtk, s1.VenusRes + s2.VenusRes, s1.MarsAtk + s2.MarsAtk, s1.MarsRes + s2.MarsRes, s1.JupiterAtk + s2.JupiterAtk, s1.JupiterRes + s2.JupiterRes, s1.MercuryAtk + s2.MercuryAtk, s1.MercuryRes + s2.MercuryRes);
        }

        public override string ToString()
        {
            return $"`VnPow: {VenusAtk} MrPow: {MarsAtk} JpPow: {JupiterAtk} McPow: {MercuryAtk}`\n" +
                $"`VnRes: {VenusRes} MrRes: {MarsRes} JpRes: {JupiterRes} McRes: {MercuryRes}`";
        }

        public string NonZerosToSTring()
        {
            return $"{((VenusAtk != 0 || VenusRes != 0) ? $"<:Venus_Element:573938340219584524> `{VenusAtk} | {VenusRes}` " : "")}" +
                $"{((MarsAtk != 0 || MarsRes != 0) ? $"<:Mars_Element:573938340307402786> `{MarsAtk} | {MarsRes}` " : "")}" +
                $"{((JupiterAtk != 0 || JupiterRes != 0) ? $"<:Jupiter_Element:573938340584488987> `{JupiterAtk} | {JupiterRes}` " : "")}" +
                $"{((MercuryAtk != 0 || MercuryRes != 0) ? $"<:Mercury_Element:573938340743872513> `{MercuryAtk} | {MercuryRes}` " : "")}";
        }

        internal int leastRes()
        {
            return (new[] { VenusRes, MarsRes, JupiterRes, MercuryRes }).Min();
        }

        internal int highestRes()
        {
            return (new[] { VenusRes, MarsRes, JupiterRes, MercuryRes }).Max();
        }

        internal int GetPower(Element e)
        {
            switch (e)
            {
                case Element.Venus: return VenusAtk;
                case Element.Mars: return MarsAtk;
                case Element.Jupiter: return JupiterAtk;
                case Element.Mercury: return MercuryAtk;
                default: return 100;
            }
        }

        internal int GetRes(Element e)
        {
            switch (e)
            {
                case Element.Venus: return VenusRes;
                case Element.Mars: return MarsRes;
                case Element.Jupiter: return JupiterRes;
                case Element.Mercury: return MercuryRes;
                default: return 100;
            }
        }
    }

    public class Stats
    {
        public int maxHP { get; set; }
        [JsonIgnore] public int HP { get; set; }
        public int maxPP { get; set; }
        [JsonIgnore] public int PP { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int Spd { get; set; }

        public Stats(int maxHP, int maxPP, int atk, int def, int spd)
        {
            this.maxHP = maxHP;
            this.maxPP = maxPP;
            HP = maxHP;
            PP = maxPP;
            Atk = atk;
            Def = def;
            Spd = spd;
        }

        public static Stats operator *(Stats s1, double factor)
        {
            return new Stats((int)(s1.maxHP * factor), (int)(s1.maxPP * factor), (int)(s1.Atk * factor), (int)(s1.Def * factor), (int)(s1.Spd * factor));
        }

        public static Stats operator *(Stats s1, Stats s2)
        {
            return new Stats(s1.maxHP * s2.maxHP, s1.maxPP * s2.maxPP, s1.Atk * s2.Atk, s1.Def * s2.Def, s1.Spd * s2.Spd);
        }

        public static Stats operator +(Stats s1, Stats s2)
        {
            return new Stats(s1.maxHP + s2.maxHP, s1.maxPP + s2.maxPP, s1.Atk + s2.Atk, s1.Def + s2.Def, s1.Spd + s2.Spd);
        }

        public override string ToString()
        {
            return $"`HP: {maxHP} Atk: {Atk} Agi: {Spd}`\n` PP: {maxPP} Def: {Def}`";
        }

        public string NonZerosToString()
        {
            return $"`{(maxHP != 0 ? $"HP: {maxHP} " : "")}" +
                $"{(maxPP != 0 ? $"PP: {maxPP} " : "")}" +
                $"{(Atk != 0 ? $"Atk: {Atk} " : "")}" +
                $"{(Def != 0 ? $"Def: {Def} " : "")}" +
                $"{(Spd != 0 ? $"Agi: {Spd} " : "")}`";
        }

        public string MultipliersToString()
        {
            return $"`{(maxHP != 100 ? $"HP: x{((double)maxHP / 100)} " : "")}" +
                $"{(maxPP != 100 ? $"PP: x{((double)maxPP / 100)} " : "")}" +
                $"{(Atk != 100 ? $"Atk: x{((double)Atk / 100)} " : "")}" +
                $"{(Def != 100 ? $"Def: x{((double)Def / 100)} " : "")}" +
                $"{(Spd != 100 ? $"Agi: x{((double)Spd / 100)} " : "")}`";
        }
    }
}
