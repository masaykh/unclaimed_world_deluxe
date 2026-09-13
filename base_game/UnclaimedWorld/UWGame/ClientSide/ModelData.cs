using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xclna.Xna.Animation;

namespace UWGame.ClientSide;

public class ModelData
{
	public Model Model;

	public float CRTDisplayScale = 1f;

	public float CRTDisplayLightIntensity = 1f;

	public float BoundingSphereRadius;

	public ModelType ModelType;

	public Vector3 ModelOffset = new Vector3(0f, 0f, 0f);

	public AttachPoint LeftHandAttachor;

	public AttachPoint RightHandAttachor;

	public AttachPoint BackAttachor;

	public AttachPoint HelmetAttachor;

	public AttachPoint DriverAttachor;

	public List<AttachPoint> PassengerAttachors;

	public AttachPoint CargoAttachor;

	public AttachPoint BackAttachee;

	public AttachPoint RightHandAttachee;

	public AttachPoint LeftHandAttachee;

	public AttachPoint BottomAttachee;

	public bool HasAircraftDucts;

	public bool HasSteerableFrontWheels;

	public bool HasEmittingParts;

	public AttachPoint GetAttachPointFromKeyName(string keyName)
	{
		if (CargoAttachor != null && keyName == CargoAttachor.KeyName)
		{
			return CargoAttachor;
		}
		if (DriverAttachor != null && keyName == DriverAttachor.KeyName)
		{
			return DriverAttachor;
		}
		foreach (AttachPoint passengerAttachor in PassengerAttachors)
		{
			if (passengerAttachor.KeyName == keyName)
			{
				return passengerAttachor;
			}
		}
		return null;
	}

	public AttachPoint GetAttachPointFromTag(string tag)
	{
		if (RightHandAttachor != null && tag == RightHandAttachor.Tag)
		{
			return RightHandAttachor;
		}
		if (LeftHandAttachor != null && tag == LeftHandAttachor.Tag)
		{
			return LeftHandAttachor;
		}
		if (BackAttachor != null && tag == BackAttachor.Tag)
		{
			return BackAttachor;
		}
		if (HelmetAttachor != null && tag == HelmetAttachor.Tag)
		{
			return HelmetAttachor;
		}
		if (CargoAttachor != null && tag == CargoAttachor.Tag)
		{
			return CargoAttachor;
		}
		if (CargoAttachor != null && tag == CargoAttachor.Tag)
		{
			return CargoAttachor;
		}
		if (DriverAttachor != null && tag == DriverAttachor.Tag)
		{
			return DriverAttachor;
		}
		foreach (AttachPoint passengerAttachor in PassengerAttachors)
		{
			if (passengerAttachor.Tag == tag)
			{
				return passengerAttachor;
			}
		}
		return null;
	}

	public AttachPoint GetAttacheePoint(AttacheePoint? attacheePointName)
	{
		if (attacheePointName == AttacheePoint.Back)
		{
			return BackAttachee;
		}
		if (attacheePointName == AttacheePoint.RightHand)
		{
			return RightHandAttachee;
		}
		if (attacheePointName == AttacheePoint.Bottom)
		{
			return BottomAttachee;
		}
		if (attacheePointName == AttacheePoint.LeftHand)
		{
			return LeftHandAttachee;
		}
		return RightHandAttachee;
	}
}
