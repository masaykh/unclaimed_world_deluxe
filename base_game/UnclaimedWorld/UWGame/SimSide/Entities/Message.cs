namespace UWGame.SimSide.Entities;

public struct Message
{
	public enum CancelJobKeepVehicle
	{
		KeepVehicle,
		LeaveVehicle
	}

	public enum MessageTypes
	{
		PathFound,
		PathNotFound,
		DistanceFound,
		BestCropFound,
		DistanceFoundNoAccess,
		CancelJobOrItemInUse,
		CancelJobForAIReset,
		OtherAgentRequestsDropItem,
		PreyIsNear,
		EntityDied,
		DrivenVehicleIsNear,
		GunshotIsNear,
		Die,
		WakeUpCombatAlert,
		StopGoalSleep,
		AlertToPresence,
		DinnerIsReady,
		PickMeUp,
		GoToRendezvousPoint,
		HopOnBoard,
		OKImOn,
		GetOff,
		Disembark,
		StartGroupMovement,
		EndGroupMovement,
		WaitForMeGroup,
		SlowdownGroup,
		Hit,
		HitAndCollapse,
		Collapse,
		WaitAndMakeRoom,
		StartConversation,
		ListenToConversation,
		Interest,
		SpeakLine
	}

	public MessageTypes MessageType;

	public object OtherInfo;

	public Entity Sender;

	public Message(Entity sender, MessageTypes type, object otherInfo)
	{
		MessageType = type;
		OtherInfo = otherInfo;
		Sender = sender;
	}

	public Message(MessageTypes type)
	{
		MessageType = type;
		OtherInfo = null;
		Sender = null;
	}
}
