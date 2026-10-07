using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Entities.Body
{
    [DebuggerDisplay("{KeyName}")]
    public class BodyLayerType: IGameData
    {
        public string KeyName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public bool DeleteRecord
        {
            get;
            set;
        }

        
        /// <summary>
        /// the relative thickness of this layer to the other layers
        /// </summary>
        public float Thickness;

        /// <summary>
        /// the resistance of this layer against various types of damage
        /// per thickness unit?
        /// high to simulate resistance against non-penetrating attacks (blunt damage)
        /// </summary>
        // PORT: [XmlIgnore], written as DamageReductionFactorEntries. XmlSerializer cannot serialize
        // an IDictionary, and refused the whole of bodyLayerTypes.xml (docs/modding.md).
        [XmlIgnore]
        public Dictionary<string, float> DamageReductionFactor;

        /// <summary>PORT: DamageReductionFactor in the XML, as key/value pairs under the same element name.</summary>
        [XmlArray("DamageReductionFactor")]
        public KVP<string, float>[] DamageReductionFactorEntries
        {
            get => ToEntries(DamageReductionFactor);
            set => DamageReductionFactor = FromEntries(value);
        }

        [XmlIgnore]
        public Dictionary<DamageType, float> DamageReductionFactorFinal;


        /// <summary>
        /// this amount is always deducted from an attack. Can nullify attacks with a damage lower than this number.
        /// high to simulate resistance against penetrating attacks (piercing)
        /// </summary>
        // PORT: as DamageReductionFactor.
        [XmlIgnore]
        public Dictionary<string, float> DamageReductionConstant;

        /// <summary>PORT: DamageReductionConstant in the XML, as key/value pairs under the same element name.</summary>
        [XmlArray("DamageReductionConstant")]
        public KVP<string, float>[] DamageReductionConstantEntries
        {
            get => ToEntries(DamageReductionConstant);
            set => DamageReductionConstant = FromEntries(value);
        }

        [XmlIgnore]
        public Dictionary<DamageType, float> DamageReductionConstantFinal;

        private static KVP<string, float>[] ToEntries(Dictionary<string, float> dictionary) =>
            dictionary?.Select(kv => new KVP<string, float>(kv.Key, kv.Value)).ToArray();

        private static Dictionary<string, float> FromEntries(KVP<string, float>[] entries) =>
            entries?.ToDictionary(kv => kv.Key, kv => kv.Value);


        public override string ToString()
        {
            return Name;
        }


        public void PostLoadContentInitialize()
        {
            DamageReductionConstantFinal = new Dictionary<DamageType, float>();
            foreach (var item in DamageReductionConstant)
            {
                DamageReductionConstantFinal.Add(GameData.Instance.AllDamageTypes[item.Key], item.Value);                
            }


            DamageReductionFactorFinal = new Dictionary<DamageType, float>();
            foreach (var item in DamageReductionFactor)
            {
                DamageReductionFactorFinal.Add(GameData.Instance.AllDamageTypes[item.Key], item.Value);
            }
        }


        #region IGameData Members


        public void Initialize()
        {
           
        }


        public void PreInitValidate(ref List<string> listOfErrors) { }
        public void PostInitValidate(ref List<string> listOfErrors)
        {          
        }
        public void PostDataCompleteInitialize()
        {
        }
        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }
        #endregion

    }
}
