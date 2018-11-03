using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;


/// <summary>
/// Contains Trial data for the Azzarano gameType.
/// </summary>
public class AzzaranoTrial : Trial
{
	/// <summary>
	/// The duration the stimulus will be shown for.
	/// </summary>
	public float duration = 0;
    public const string ATTRIBUTE_POSITIONX = "positionX";
    public const string ATTRIBUTE_POSITIONY = "positionY";
    public const string ATTRIBUTE_RED = "red";
    public float positionX = 0;
    public float positionY = 0;
    public bool red = false;


	#region ACCESSORS

	public float Duration
	{
		get
		{
			return duration;
		}
	}
    public float PositionX
    {
        get
        {
            return positionX;
        }
    }
    public float PositionY
    {
        get
        {
            return positionY;
        }
    }
    public bool Red
    {
        get
        {
            return red;
        }
    }


    #endregion


    public AzzaranoTrial(SessionData data, XmlElement n = null) 
		: base(data, n)
	{
	}
	

	/// <summary>
	/// Parses Game specific variables for this Trial from the given XmlElement.
	/// If no parsable attributes are found, or fail, then it will generate some from the given GameData.
	/// Used when parsing a Trial that IS defined in the Session file.
	/// </summary>
	public override void ParseGameSpecificVars(XmlNode n, SessionData session)
	{
		base.ParseGameSpecificVars(n, session);

		AzzaranoData data = (AzzaranoData)(session.gameData);
		if (!XMLUtil.ParseAttribute(n, AzzaranoData.ATTRIBUTE_DURATION, ref duration, true))
		{
			duration = data.GeneratedDuration;
		}

        XMLUtil.ParseAttribute(n, ATTRIBUTE_POSITIONX, ref positionX);
        XMLUtil.ParseAttribute(n, ATTRIBUTE_POSITIONY, ref positionY);
        XMLUtil.ParseAttribute(n, ATTRIBUTE_RED, ref red);
    }


	/// <summary>
	/// Writes any tracked variables to the given XElement.
	/// </summary>
	public override void WriteOutputData(ref XElement elem)
	{
		base.WriteOutputData(ref elem);
		XMLUtil.CreateAttribute(AzzaranoData.ATTRIBUTE_DURATION, duration.ToString(), ref elem);
	}
}
