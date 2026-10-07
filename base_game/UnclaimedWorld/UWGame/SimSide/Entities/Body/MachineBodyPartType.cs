using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Items;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.Entities.Body
{
   // [XmlInclude(typeof(BiologicalBodyPartType)), XmlInclude(typeof(BodyPartType))]
    // PORT: not IXmlSerializable. XmlSerializer will not take an IXmlSerializable class as the
    // derived type an [XmlInclude] names, so every body with a machine part - robots, sentries -
    // made bodyTypes.xml unexportable ("MachineBodyPartType may not be used in this context"),
    // and EntityTypes, AttackTypes and FilterSettingTypes failed after it on the bodies it lacked.
    // Its own field is one float; the inherited ones serialize as they do for
    // BiologicalBodyPartType, which never had a proxy. The proxy (WriteXml/ReadXml and
    // _proxyData) went with it; tools/build/20-generate-xml-proxies.sh drops the generated one.
    public class MachineBodyPartType: BodyPartType
    {
       // public BodyPartType Parent;


       // public Dictionary<EntityType, int> MadeOf; 

        /// <summary>
        /// if true, all BodyParts inside will get protection from the elements
        /// </summary>
        //public bool IsEnclosed;
       // public bool IsInternal;

        /// <summary>
        /// time needed to put this body part together with the others!
        /// </summary>
        public float ManSecondsOfWorkNeeded;

        
        // the entity functions affected by this body part.
       // public MachineBodyPartFunction[] MachineFunctions;

    }
}
