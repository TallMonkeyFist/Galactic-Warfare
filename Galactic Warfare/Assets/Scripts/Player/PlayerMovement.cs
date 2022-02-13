using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
	[Header("References")]
	[Tooltip("Character controller for the player")]
	[SerializeField] private CharacterController controller = null;
	[Tooltip("Head/Look transform of the player")]
	[SerializeField] private Transform headTransform = null;
	[Tooltip("Right axis transform of the player")]
	[SerializeField] private Transform weaponTransform = null;


	[Header("Horizontal Settings")]
	[Tooltip("Default movement speed")]
	[SerializeField] private float moveSpeed = 6.0f;
	[Tooltip("Sprint speed multiplier")]
	[SerializeField] private float sprintMultiplier = 1.4f;
	[Tooltip("How long the player can sprint for (seconds)")]
	[SerializeField] private float sprintTime = 6.0f;
	[Tooltip("Time it takes for stamina to recharge from 0 (seconds)")]
	[SerializeField] private float sprintChargeTime = 3.0f;
	[Tooltip("Percentage of stamina required to start sprinting")]
	[Range(0, 1)]
	[SerializeField] private float sprintStartCharge = 0.1f;

	[Header("Vertical Settings")]
	[Tooltip("Some number that represents jump height")]
	[SerializeField] private float jumpHeight = 3.0f;
	[Tooltip("Force of gravity being applied to the player")]
	[SerializeField] private float gravity = 20.0f;

	[Header("Player Rotation")]
	[Tooltip("Player mouse speed")]
	[SerializeField] private float mouseSensitivity = 250.0f;

	[Header("Force")]
	[SerializeField] private float mass = 62.0f;


	[SyncVar(hook = nameof(SyncServerLastMove))]
	private PlayerMoveState ServerLastMove;
	private float ServerLastCommandReceived;

	public bool InputEnabled;

	//Player sprint variables
	float stamina;
	float minStamina;
	float sprintCharge;
	bool sprinting;

	//Player jump variables
	Vector3 downForce;
	Vector3 lastDirection;

	//Player rotation variables
	public float xAxis { get; private set; }
	public Transform LookTransform { get { return headTransform; } }

	[System.Serializable]
	public struct MoveCommand
	{
		public bool forward;
		public bool backward;
		public bool left;
		public bool right;
		public bool jump;
		public bool sprint;
		public Vector3 rightDirection;
		public Vector3 forwardDirection;
		public float deltaTime;
		public double serverTime;
		public Vector3 position;
		public bool simulated;
		public uint moveNumber;

		public MoveCommand(bool _forward, bool _backward, bool _left, bool _right, bool _jump, bool _sprint, Vector3 _rightDir, Vector3 _forwardDir, float _deltaTime, double _time, uint _number)
		{
			forward = _forward;
			backward = _backward;
			left = _left;
			right = _right;
			jump = _jump;
			sprint = _sprint;
			rightDirection = _rightDir;
			forwardDirection = _forwardDir;
			serverTime = _time;
			position = Vector3.positiveInfinity;
			deltaTime = _deltaTime;
			simulated = false;
			moveNumber = _number;
		}

		public MoveCommand(float _deltaTime, double _time, uint _number)
		{
			forward = false;
			backward = false;
			left = false;
			right = false;
			jump = false;
			sprint = false;
			rightDirection = Vector3.right;
			forwardDirection = Vector3.forward;
			serverTime = _time;
			position = Vector3.positiveInfinity;
			deltaTime = _deltaTime;
			simulated = false;
			moveNumber = _number;
		}
	}

	[System.Serializable]
	public struct PlayerMoveState
	{
		public Vector3 position;
		public Vector3 lastDirection;
		public float stamina;
		public float gravity;
		public bool sprinting;
		public uint moveNumber;

		public PlayerMoveState(Vector3 _pos, Vector3 _dir, float _stamina, float _gravity, uint _number, bool _sprint)
		{
			position = _pos;
			stamina = _stamina;
			gravity = _gravity;
			moveNumber = _number;
			sprinting = _sprint;
			lastDirection = _dir;
		}

		public static PlayerMoveState Create()
		{
			return new PlayerMoveState(Vector3.positiveInfinity, Vector3.forward, -1, -1, 0, false);
		}
	}

	private void Start()
	{
		xAxis = 0;
		stamina = sprintTime;
		minStamina = sprintTime * sprintStartCharge;
		sprintCharge = sprintTime / sprintChargeTime;
	}

	private static Vector3 GetMovementDirection(bool _forward, bool _backward, bool _left, bool _right, Vector3 _rightDir, Vector3 _forwardDir)
	{
		int x = 0;
		int z = 0;

		if (_forward)
		{
			z++;
		}
		if (_backward)
		{
			z--;
		}
		if (_left)
		{
			x--;
		}
		if (_right)
		{
			x++;
		}

		Vector3 direction = (_rightDir * x + _forwardDir * z).normalized;

		return direction;
	}

	private void Move(bool _forward, bool _backward, bool _left, bool _right, bool _wantsToJump, bool _wantsToSprint, Vector3 _rightDir, Vector3 _forwardDir, float deltaTime)
	{
		Vector3 direction = GetMovementDirection(_forward, _backward, _left, _right, _rightDir, _forwardDir);

		float speed = moveSpeed;

		if (controller.isGrounded)
		{
			downForce.y = -2.0f;

			if (_wantsToJump)
			{
				downForce.y = Mathf.Sqrt(jumpHeight * 2.0f * gravity);
			}
			lastDirection = direction;

			if (_wantsToSprint)
			{
				if (sprinting)
				{
					if (stamina > 0)
					{
						speed *= sprintMultiplier;
					}
					else
					{
						sprinting = false;
					}
				}
				else
				{
					if (stamina > minStamina)
					{
						speed *= sprintMultiplier;
						sprinting = true;
					}
				}
			}
			else
			{
				sprinting = false;
			}
		}
		else
		{
			direction = Vector3.ClampMagnitude((direction * .25f + lastDirection), 1.0f);
		}

		if (sprinting)
		{
			stamina = Mathf.Clamp(stamina - deltaTime, 0, sprintTime);

			if (isClient)
			{
				ClientOnStaminaChanged?.Invoke(stamina, sprintTime);
			}
		}
		else if (stamina < sprintTime)
		{
			stamina = Mathf.Clamp(stamina + sprintCharge * deltaTime, 0, sprintTime);

			if (isClient)
			{
				ClientOnStaminaChanged?.Invoke(stamina, sprintTime);
			}
		}

		downForce.y -= gravity * deltaTime;

		controller.Move(direction * speed * deltaTime + downForce * deltaTime);
	}

	public float GetStamina()
	{
		return stamina;
	}

	public float GetMaxStamina()
	{
		return sprintTime;
	}

	private void Update()
	{
		ClientUpdate();
	}

	private void FixedUpdate()
	{
		ServerUpdate();
	}

	#region Server

	private Queue<MoveCommand> serverMoveCommands = new Queue<MoveCommand>();
	private uint serverMoveNumber = 0;

	public override void OnStartServer()
	{
		stamina = sprintTime;
		minStamina = sprintTime * sprintStartCharge;
		sprintCharge = sprintTime / sprintChargeTime;
		downForce.y = -2.0f;

		PlayerMoveState state = new PlayerMoveState(controller.transform.position, controller.transform.forward, stamina, downForce.y, 0, false);
		ServerLastMove = state;

		RpcForcePosition(controller.transform.position);

		ServerLastCommandReceived = Time.time;
	}

	[Command]
	private void CmdMovePlayer(MoveCommand command)
	{
		command.moveNumber = serverMoveNumber++;
		serverMoveCommands.Enqueue(command);
	}

	[Command]
	private void CmdSetRotation(float _head, float _rotate, float _weapon)
	{
		RpcClientHandleRotation(_head, _rotate, _weapon);
	}

	[ServerCallback]
	private void ServerUpdate()
	{
		MoveCommand lastCommand = new MoveCommand();
		bool commandSet = false;

		while (serverMoveCommands.Count > 0)
		{
			commandSet = true;
			MoveCommand moveCommand = serverMoveCommands.Dequeue();

			Move(moveCommand.forward, moveCommand.backward, moveCommand.left,
				 moveCommand.right, moveCommand.jump, moveCommand.sprint,
				 moveCommand.rightDirection, moveCommand.forwardDirection,
				 moveCommand.deltaTime);

			lastCommand = moveCommand;
		}

		if (commandSet)
		{
			PlayerMoveState state = new PlayerMoveState(controller.transform.position, lastDirection, stamina, downForce.y, lastCommand.moveNumber, sprinting);
			ServerLastMove = state;
		}
	}

	[Command]
	public void CmdSetInput(bool inputEnabled)
	{
		InputEnabled = inputEnabled;
	}

	[Server]
	public void ServerSetInput(bool inputEnabled)
	{
		TargetSetInput(inputEnabled);
	}

	#endregion

	#region Client

	public event Action<float, float> ClientOnStaminaChanged;

	private PlayerMoveState ClientLastState;
	private List<MoveCommand> moveCommands = new List<MoveCommand>();
	private uint clientLastCommand = 0;
	private bool sync = false;
	private double lastTime;

	[ClientCallback]
	private void ClientUpdate()
	{
		//Prevents host from running this because they are server and client
		if (isServer && !hasAuthority) { return; }

		if (isServer)
		{
			PollInputs();
			return;
		}

		if (!hasAuthority)
		{
			LerpToServerPosition();
			return;
		}

		if (sync)
		{
			sync = false;
			SyncServerMove();
		}

		PollInputs();
		ClientMovePrediction();
	}

	[Client]
	private void LerpToServerPosition()
	{
		Vector3 lerpedPosition = Vector3.Lerp(controller.transform.position, ServerLastMove.position, 0.5f);
		controller.enabled = false;
		controller.transform.position = lerpedPosition;
		controller.enabled = true;
	}

	[Client]
	private void PollInputs()
	{
		MoveCommand command;

		float deltaTime = (float)(System.DateTime.Now.TimeOfDay.TotalSeconds - lastTime);
		lastTime = System.DateTime.Now.TimeOfDay.TotalSeconds;

		//Remove commands that don't change the player position
		if(deltaTime == 0)
		{
			return;
		}

		if (!InputEnabled)
		{
			command = new MoveCommand(deltaTime, NetworkTime.time, clientLastCommand++);

			ClientRotate(0, 0);
			CmdMovePlayer(command);
			ClientMove(command);
			return;
		}

		//Rotation (must do before setting move command directions
		float mouseX = Input.GetAxisRaw("Mouse X");
		float mouseY = Input.GetAxisRaw("Mouse Y");

		ClientRotate(mouseX, mouseY);

		//Movement
		bool _forward = Input.GetKey(KeyCode.W);
		bool _backward = Input.GetKey(KeyCode.S);
		bool _left = Input.GetKey(KeyCode.A);
		bool _right = Input.GetKey(KeyCode.D);
		bool _jump = Input.GetKey(KeyCode.Space);
		bool _sprint = Input.GetKey(KeyCode.LeftShift);

		command = new MoveCommand(_forward, _backward, _left,
			_right, _jump, _sprint, controller.transform.right,
			controller.transform.forward, deltaTime, NetworkTime.time, clientLastCommand++);

		CmdMovePlayer(command);
		ClientMove(command);
	}

	[Client]
	private void ClientMovePrediction()
	{
		//Prevents the host from doing client side prediction,
		//Host does not need to because they would then move twice
		if (isServer)
		{
			return;
		}

		controller.enabled = false;
		controller.transform.position = ServerLastMove.position;
		controller.enabled = true;

		stamina = ServerLastMove.stamina;
		downForce.y = ServerLastMove.gravity;
		lastDirection = ServerLastMove.lastDirection;
		sprinting = ServerLastMove.sprinting;

		ClientOnStaminaChanged?.Invoke(stamina, sprintTime);

		for (int i = 0; i < moveCommands.Count; i++)
		{
			MoveCommand mc = moveCommands[i];

			if(mc.moveNumber <= ServerLastMove.moveNumber)
			{
				continue; 
			}

			Move(mc.forward, mc.backward, mc.left, 
				mc.right, mc.jump, mc.sprint, 
				mc.rightDirection, mc.forwardDirection, 
				mc.deltaTime);

			mc.position = controller.transform.position;

			if (!mc.simulated)
			{
				mc.simulated = true;
				//Play sound effects
			}

			moveCommands[i] = mc;

			ClientLastState = new PlayerMoveState(
				controller.transform.position, lastDirection, stamina, 
				gravity, mc.moveNumber, sprinting);
		}
	}

	[Client]
	private void ClientMove(MoveCommand moveCommand)
	{

		//Prevents the host from running this code and moving twice every tick
		if (isServer)
		{
			return;
		}

		moveCommands.Add(moveCommand);
	}

	[Client]
	private void ClientRotate(float x, float y)
	{
		controller.transform.Rotate(Vector3.up, x * mouseSensitivity * Time.deltaTime);

		xAxis = Mathf.Clamp(xAxis - (y * mouseSensitivity * Time.deltaTime), -80.0f, 70.0f);

		weaponTransform.localRotation = Quaternion.Euler(xAxis, 0.0f, 0.0f);
		headTransform.localRotation = weaponTransform.localRotation;

		CmdSetRotation(headTransform.localEulerAngles.x, controller.transform.localEulerAngles.y, weaponTransform.localEulerAngles.x);
	}

	[Client]
	private void SyncServerMove()
	{
		PlayerMoveState _newState = ServerLastMove;

		//Prevents host from running this because they are a server and client
		if (isServer) { return; }

		if (hasAuthority)
		{
			if (moveCommands.Count == 0 && (controller.transform.position - _newState.position).sqrMagnitude < 0.00001f)
			{
				return;
			}
			else if(moveCommands.Count == 0)
			{
				Debug.Log($"Conflict with a size difference of {(controller.transform.position - _newState.position).magnitude}"); 
				
				controller.enabled = false;
				controller.transform.position = _newState.position;
				controller.enabled = true;
				return;
			}

			for (int i = 0; i < moveCommands.Count; i++)
			{
				MoveCommand moveCommand = moveCommands[i];

				if(moveCommand.moveNumber == _newState.moveNumber)
				{
					if ((moveCommand.position - _newState.position).sqrMagnitude < 0.00001f)
					{
						moveCommands.RemoveRange(0, i + 1);

						return;
					}
					else
					{
						Debug.Log($"Conflict with a size difference of {(moveCommand.position - _newState.position).magnitude}");

						controller.enabled = false;
						controller.transform.position = _newState.position;
						controller.enabled = true;

						moveCommands.RemoveRange(0, i + 1);

						return;
					}
				}
			}

			Debug.Log($"Conflict with a size difference of {(_newState.position - moveCommands[moveCommands.Count - 1].position).magnitude}\nThis should not happen");

			moveCommands.Clear();

			controller.enabled = false;
			controller.transform.position = _newState.position;
			controller.enabled = true;
		}
	}

	/*[Client]
	private void SyncServerMove()
	{
		PlayerMoveState _newState = ServerLastMove;

		//Prevents host from running this because they are a server and client
		if (isServer) { return; }

		if (hasAuthority)
		{
			if (moveCommands.Count == 0 && (controller.transform.position - _newState.position).sqrMagnitude < 0.0001f)
			{
				return;
			}
			for (int i = 0; i < moveCommands.Count; i++)
			{
				MoveCommand moveCommand = moveCommands[i];
				if ((_newState.position - moveCommand.position).sqrMagnitude < 0.0001f)
				{

					for (int j = i + 1; j < moveCommands.Count; j++)
					{
						MoveCommand toCheck = moveCommands[j];
						if ((_newState.position - toCheck.position).sqrMagnitude < 0.0001f)
						{
							i++;
						}
						else
						{
							break;
						}
					}

					moveCommands.RemoveRange(0, i + 1);

					return;
				}
			}

			//Server and client are not synced, reset the client position
			Debug.Log($"Conflict with a size difference of {(_newState.position - moveCommands[moveCommands.Count - 1].position).magnitude}");

			moveCommands.Clear();

			controller.enabled = false;
			controller.transform.position = _newState.position;
			controller.enabled = true;
			return;
		}
	}*/

	[Client]
	private void SyncServerLastMove(PlayerMoveState _oldState, PlayerMoveState _newState)
	{   
		if(_newState.position == Vector3.positiveInfinity) { return; }

		//Prevents host from running this because they are a server and client
		if(isServer) { return; }

		if (hasAuthority)
		{
			sync = true;
			return;
		}

		if(_oldState.position == Vector3.positiveInfinity) 
		{

			controller.enabled = false;
			controller.transform.position = _newState.position;
			controller.enabled = true;
			return;
		}

		controller.enabled = false;
		controller.transform.position = _oldState.position;
		controller.enabled = true;
	}

	[ClientRpc]
	private void RpcClientHandleRotation(float _head, float _rotate, float _weapon)
	{
		if (hasAuthority) { return; }

		headTransform.localRotation = Quaternion.Euler(_head, 0, 0);
		controller.transform.localRotation = Quaternion.Euler(0, _rotate, 0);
		weaponTransform.localRotation = Quaternion.Euler(_weapon, 0, 0);
	}

	[Client]
	private void ClientSetInput(bool inputEnabled)
	{
		InputEnabled = inputEnabled;
		CmdSetInput(inputEnabled);
	}

	[TargetRpc]
	private void TargetSetInput(bool inputEnabled)
	{
		InputEnabled = inputEnabled;
	}

	[ClientRpc]
	private void RpcForcePosition(Vector3 position)
	{
		if(hasAuthority || isServer) { return; }

		controller.enabled = false;
		controller.transform.position = position;
		controller.enabled = true;
	}

	[Client]
	public override void OnStartClient()
	{
		if(ServerLastMove.position != Vector3.positiveInfinity)
		{
			controller.enabled = false;
			controller.transform.position = ServerLastMove.position;
			controller.enabled = true;
		}

		lastTime = System.DateTime.Now.TimeOfDay.TotalSeconds;
	}
	#endregion
}