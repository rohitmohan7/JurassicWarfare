using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0.15,0.15,0,0,0.15,0.15,0,0,0]")]
	public partial class PlayerCreatureNetworkObject : NetworkObject
	{
		public const int IDENTITY = 7;

		private byte[] _dirtyFields = new byte[2];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		[ForgeGeneratedField]
		private Vector3 _position;
		public event FieldEvent<Vector3> positionChanged;
		public InterpolateVector3 positionInterpolation = new InterpolateVector3() { LerpT = 0.15f, Enabled = true };
		public Vector3 position
		{
			get { return _position; }
			set
			{
				// Don't do anything if the value is the same
				if (_position == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_position = value;
				hasDirtyFields = true;
			}
		}

		public void SetpositionDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_position(ulong timestep)
		{
			if (positionChanged != null) positionChanged(_position, timestep);
			if (fieldAltered != null) fieldAltered("position", _position, timestep);
		}
		[ForgeGeneratedField]
		private Quaternion _rotation;
		public event FieldEvent<Quaternion> rotationChanged;
		public InterpolateQuaternion rotationInterpolation = new InterpolateQuaternion() { LerpT = 0.15f, Enabled = true };
		public Quaternion rotation
		{
			get { return _rotation; }
			set
			{
				// Don't do anything if the value is the same
				if (_rotation == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_rotation = value;
				hasDirtyFields = true;
			}
		}

		public void SetrotationDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_rotation(ulong timestep)
		{
			if (rotationChanged != null) rotationChanged(_rotation, timestep);
			if (fieldAltered != null) fieldAltered("rotation", _rotation, timestep);
		}
		[ForgeGeneratedField]
		private int _move;
		public event FieldEvent<int> moveChanged;
		public Interpolated<int> moveInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int move
		{
			get { return _move; }
			set
			{
				// Don't do anything if the value is the same
				if (_move == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x4;
				_move = value;
				hasDirtyFields = true;
			}
		}

		public void SetmoveDirty()
		{
			_dirtyFields[0] |= 0x4;
			hasDirtyFields = true;
		}

		private void RunChange_move(ulong timestep)
		{
			if (moveChanged != null) moveChanged(_move, timestep);
			if (fieldAltered != null) fieldAltered("move", _move, timestep);
		}
		[ForgeGeneratedField]
		private bool _attack;
		public event FieldEvent<bool> attackChanged;
		public Interpolated<bool> attackInterpolation = new Interpolated<bool>() { LerpT = 0f, Enabled = false };
		public bool attack
		{
			get { return _attack; }
			set
			{
				// Don't do anything if the value is the same
				if (_attack == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x8;
				_attack = value;
				hasDirtyFields = true;
			}
		}

		public void SetattackDirty()
		{
			_dirtyFields[0] |= 0x8;
			hasDirtyFields = true;
		}

		private void RunChange_attack(ulong timestep)
		{
			if (attackChanged != null) attackChanged(_attack, timestep);
			if (fieldAltered != null) fieldAltered("attack", _attack, timestep);
		}
		[ForgeGeneratedField]
		private float _turn;
		public event FieldEvent<float> turnChanged;
		public InterpolateFloat turnInterpolation = new InterpolateFloat() { LerpT = 0.15f, Enabled = true };
		public float turn
		{
			get { return _turn; }
			set
			{
				// Don't do anything if the value is the same
				if (_turn == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x10;
				_turn = value;
				hasDirtyFields = true;
			}
		}

		public void SetturnDirty()
		{
			_dirtyFields[0] |= 0x10;
			hasDirtyFields = true;
		}

		private void RunChange_turn(ulong timestep)
		{
			if (turnChanged != null) turnChanged(_turn, timestep);
			if (fieldAltered != null) fieldAltered("turn", _turn, timestep);
		}
		[ForgeGeneratedField]
		private float _pitch;
		public event FieldEvent<float> pitchChanged;
		public InterpolateFloat pitchInterpolation = new InterpolateFloat() { LerpT = 0.15f, Enabled = true };
		public float pitch
		{
			get { return _pitch; }
			set
			{
				// Don't do anything if the value is the same
				if (_pitch == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x20;
				_pitch = value;
				hasDirtyFields = true;
			}
		}

		public void SetpitchDirty()
		{
			_dirtyFields[0] |= 0x20;
			hasDirtyFields = true;
		}

		private void RunChange_pitch(ulong timestep)
		{
			if (pitchChanged != null) pitchChanged(_pitch, timestep);
			if (fieldAltered != null) fieldAltered("pitch", _pitch, timestep);
		}
		[ForgeGeneratedField]
		private int _idle;
		public event FieldEvent<int> idleChanged;
		public Interpolated<int> idleInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int idle
		{
			get { return _idle; }
			set
			{
				// Don't do anything if the value is the same
				if (_idle == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x40;
				_idle = value;
				hasDirtyFields = true;
			}
		}

		public void SetidleDirty()
		{
			_dirtyFields[0] |= 0x40;
			hasDirtyFields = true;
		}

		private void RunChange_idle(ulong timestep)
		{
			if (idleChanged != null) idleChanged(_idle, timestep);
			if (fieldAltered != null) fieldAltered("idle", _idle, timestep);
		}
		[ForgeGeneratedField]
		private bool _onground;
		public event FieldEvent<bool> ongroundChanged;
		public Interpolated<bool> ongroundInterpolation = new Interpolated<bool>() { LerpT = 0f, Enabled = false };
		public bool onground
		{
			get { return _onground; }
			set
			{
				// Don't do anything if the value is the same
				if (_onground == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x80;
				_onground = value;
				hasDirtyFields = true;
			}
		}

		public void SetongroundDirty()
		{
			_dirtyFields[0] |= 0x80;
			hasDirtyFields = true;
		}

		private void RunChange_onground(ulong timestep)
		{
			if (ongroundChanged != null) ongroundChanged(_onground, timestep);
			if (fieldAltered != null) fieldAltered("onground", _onground, timestep);
		}
		[ForgeGeneratedField]
		private bool _jump;
		public event FieldEvent<bool> jumpChanged;
		public Interpolated<bool> jumpInterpolation = new Interpolated<bool>() { LerpT = 0f, Enabled = false };
		public bool jump
		{
			get { return _jump; }
			set
			{
				// Don't do anything if the value is the same
				if (_jump == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[1] |= 0x1;
				_jump = value;
				hasDirtyFields = true;
			}
		}

		public void SetjumpDirty()
		{
			_dirtyFields[1] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_jump(ulong timestep)
		{
			if (jumpChanged != null) jumpChanged(_jump, timestep);
			if (fieldAltered != null) fieldAltered("jump", _jump, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			positionInterpolation.current = positionInterpolation.target;
			rotationInterpolation.current = rotationInterpolation.target;
			moveInterpolation.current = moveInterpolation.target;
			attackInterpolation.current = attackInterpolation.target;
			turnInterpolation.current = turnInterpolation.target;
			pitchInterpolation.current = pitchInterpolation.target;
			idleInterpolation.current = idleInterpolation.target;
			ongroundInterpolation.current = ongroundInterpolation.target;
			jumpInterpolation.current = jumpInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _position);
			UnityObjectMapper.Instance.MapBytes(data, _rotation);
			UnityObjectMapper.Instance.MapBytes(data, _move);
			UnityObjectMapper.Instance.MapBytes(data, _attack);
			UnityObjectMapper.Instance.MapBytes(data, _turn);
			UnityObjectMapper.Instance.MapBytes(data, _pitch);
			UnityObjectMapper.Instance.MapBytes(data, _idle);
			UnityObjectMapper.Instance.MapBytes(data, _onground);
			UnityObjectMapper.Instance.MapBytes(data, _jump);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_position = UnityObjectMapper.Instance.Map<Vector3>(payload);
			positionInterpolation.current = _position;
			positionInterpolation.target = _position;
			RunChange_position(timestep);
			_rotation = UnityObjectMapper.Instance.Map<Quaternion>(payload);
			rotationInterpolation.current = _rotation;
			rotationInterpolation.target = _rotation;
			RunChange_rotation(timestep);
			_move = UnityObjectMapper.Instance.Map<int>(payload);
			moveInterpolation.current = _move;
			moveInterpolation.target = _move;
			RunChange_move(timestep);
			_attack = UnityObjectMapper.Instance.Map<bool>(payload);
			attackInterpolation.current = _attack;
			attackInterpolation.target = _attack;
			RunChange_attack(timestep);
			_turn = UnityObjectMapper.Instance.Map<float>(payload);
			turnInterpolation.current = _turn;
			turnInterpolation.target = _turn;
			RunChange_turn(timestep);
			_pitch = UnityObjectMapper.Instance.Map<float>(payload);
			pitchInterpolation.current = _pitch;
			pitchInterpolation.target = _pitch;
			RunChange_pitch(timestep);
			_idle = UnityObjectMapper.Instance.Map<int>(payload);
			idleInterpolation.current = _idle;
			idleInterpolation.target = _idle;
			RunChange_idle(timestep);
			_onground = UnityObjectMapper.Instance.Map<bool>(payload);
			ongroundInterpolation.current = _onground;
			ongroundInterpolation.target = _onground;
			RunChange_onground(timestep);
			_jump = UnityObjectMapper.Instance.Map<bool>(payload);
			jumpInterpolation.current = _jump;
			jumpInterpolation.target = _jump;
			RunChange_jump(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _position);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _rotation);
			if ((0x4 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _move);
			if ((0x8 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _attack);
			if ((0x10 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _turn);
			if ((0x20 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _pitch);
			if ((0x40 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _idle);
			if ((0x80 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _onground);
			if ((0x1 & _dirtyFields[1]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _jump);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (positionInterpolation.Enabled)
				{
					positionInterpolation.target = UnityObjectMapper.Instance.Map<Vector3>(data);
					positionInterpolation.Timestep = timestep;
				}
				else
				{
					_position = UnityObjectMapper.Instance.Map<Vector3>(data);
					RunChange_position(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (rotationInterpolation.Enabled)
				{
					rotationInterpolation.target = UnityObjectMapper.Instance.Map<Quaternion>(data);
					rotationInterpolation.Timestep = timestep;
				}
				else
				{
					_rotation = UnityObjectMapper.Instance.Map<Quaternion>(data);
					RunChange_rotation(timestep);
				}
			}
			if ((0x4 & readDirtyFlags[0]) != 0)
			{
				if (moveInterpolation.Enabled)
				{
					moveInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					moveInterpolation.Timestep = timestep;
				}
				else
				{
					_move = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_move(timestep);
				}
			}
			if ((0x8 & readDirtyFlags[0]) != 0)
			{
				if (attackInterpolation.Enabled)
				{
					attackInterpolation.target = UnityObjectMapper.Instance.Map<bool>(data);
					attackInterpolation.Timestep = timestep;
				}
				else
				{
					_attack = UnityObjectMapper.Instance.Map<bool>(data);
					RunChange_attack(timestep);
				}
			}
			if ((0x10 & readDirtyFlags[0]) != 0)
			{
				if (turnInterpolation.Enabled)
				{
					turnInterpolation.target = UnityObjectMapper.Instance.Map<float>(data);
					turnInterpolation.Timestep = timestep;
				}
				else
				{
					_turn = UnityObjectMapper.Instance.Map<float>(data);
					RunChange_turn(timestep);
				}
			}
			if ((0x20 & readDirtyFlags[0]) != 0)
			{
				if (pitchInterpolation.Enabled)
				{
					pitchInterpolation.target = UnityObjectMapper.Instance.Map<float>(data);
					pitchInterpolation.Timestep = timestep;
				}
				else
				{
					_pitch = UnityObjectMapper.Instance.Map<float>(data);
					RunChange_pitch(timestep);
				}
			}
			if ((0x40 & readDirtyFlags[0]) != 0)
			{
				if (idleInterpolation.Enabled)
				{
					idleInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					idleInterpolation.Timestep = timestep;
				}
				else
				{
					_idle = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_idle(timestep);
				}
			}
			if ((0x80 & readDirtyFlags[0]) != 0)
			{
				if (ongroundInterpolation.Enabled)
				{
					ongroundInterpolation.target = UnityObjectMapper.Instance.Map<bool>(data);
					ongroundInterpolation.Timestep = timestep;
				}
				else
				{
					_onground = UnityObjectMapper.Instance.Map<bool>(data);
					RunChange_onground(timestep);
				}
			}
			if ((0x1 & readDirtyFlags[1]) != 0)
			{
				if (jumpInterpolation.Enabled)
				{
					jumpInterpolation.target = UnityObjectMapper.Instance.Map<bool>(data);
					jumpInterpolation.Timestep = timestep;
				}
				else
				{
					_jump = UnityObjectMapper.Instance.Map<bool>(data);
					RunChange_jump(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (positionInterpolation.Enabled && !positionInterpolation.current.UnityNear(positionInterpolation.target, 0.0015f))
			{
				_position = (Vector3)positionInterpolation.Interpolate();
				//RunChange_position(positionInterpolation.Timestep);
			}
			if (rotationInterpolation.Enabled && !rotationInterpolation.current.UnityNear(rotationInterpolation.target, 0.0015f))
			{
				_rotation = (Quaternion)rotationInterpolation.Interpolate();
				//RunChange_rotation(rotationInterpolation.Timestep);
			}
			if (moveInterpolation.Enabled && !moveInterpolation.current.UnityNear(moveInterpolation.target, 0.0015f))
			{
				_move = (int)moveInterpolation.Interpolate();
				//RunChange_move(moveInterpolation.Timestep);
			}
			if (attackInterpolation.Enabled && !attackInterpolation.current.UnityNear(attackInterpolation.target, 0.0015f))
			{
				_attack = (bool)attackInterpolation.Interpolate();
				//RunChange_attack(attackInterpolation.Timestep);
			}
			if (turnInterpolation.Enabled && !turnInterpolation.current.UnityNear(turnInterpolation.target, 0.0015f))
			{
				_turn = (float)turnInterpolation.Interpolate();
				//RunChange_turn(turnInterpolation.Timestep);
			}
			if (pitchInterpolation.Enabled && !pitchInterpolation.current.UnityNear(pitchInterpolation.target, 0.0015f))
			{
				_pitch = (float)pitchInterpolation.Interpolate();
				//RunChange_pitch(pitchInterpolation.Timestep);
			}
			if (idleInterpolation.Enabled && !idleInterpolation.current.UnityNear(idleInterpolation.target, 0.0015f))
			{
				_idle = (int)idleInterpolation.Interpolate();
				//RunChange_idle(idleInterpolation.Timestep);
			}
			if (ongroundInterpolation.Enabled && !ongroundInterpolation.current.UnityNear(ongroundInterpolation.target, 0.0015f))
			{
				_onground = (bool)ongroundInterpolation.Interpolate();
				//RunChange_onground(ongroundInterpolation.Timestep);
			}
			if (jumpInterpolation.Enabled && !jumpInterpolation.current.UnityNear(jumpInterpolation.target, 0.0015f))
			{
				_jump = (bool)jumpInterpolation.Interpolate();
				//RunChange_jump(jumpInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[2];

		}

		public PlayerCreatureNetworkObject() : base() { Initialize(); }
		public PlayerCreatureNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public PlayerCreatureNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
