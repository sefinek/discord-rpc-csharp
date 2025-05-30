﻿namespace DiscordRPC.Message;

/// <summary>
///     Called when the IPC has closed.
/// </summary>
public class CloseMessage : IMessage
{
	internal CloseMessage()
	{
	}

	internal CloseMessage(string reason)
	{
		Reason = reason;
	}

	/// <summary>
	///     The type of message
	/// </summary>
	public override MessageType Type => MessageType.Close;

	/// <summary>
	///     The reason for the close
	/// </summary>
	public string Reason { get; internal set; }

	/// <summary>
	///     The closure code
	/// </summary>
	public int Code { get; internal set; }
}
