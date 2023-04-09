using Sandbox;
using System;
using System.Linq;

namespace ExileBox;

public partial class PawnCamera : EntityComponent<Pawn>, ISingletonComponent
{
	protected Vector2 CameraDistance => new( 125, 1000 );
	protected Vector2 PitchClamp => new( 40, 40 );

	public object Main { get; internal set; }

	float OrbitDistance = 500f;
	Angles OrbitAngles = Angles.Zero;

	protected static Vector3 IntersectPlane( Vector3 pos, Vector3 dir, float z )
	{
		float a = (z - pos.z) / dir.z;
		return new( dir.x * a + pos.x, dir.y * a + pos.y, z );
	}

	protected static Rotation LookAt( Vector3 targetPosition, Vector3 position )
	{
		var targetDelta = (targetPosition - position);
		var direction = targetDelta.Normal;

		return Rotation.From( new Angles(
			((float)Math.Asin( direction.z )).RadianToDegree() * -1.0f,
			((float)Math.Atan2( direction.y, direction.x )).RadianToDegree(),
			0.0f ) );
	}

	public void Update()
	{
		var pawn = Entity;
		if ( !pawn.IsValid() )
			return;

		Camera.Position = pawn.Position;
		Vector3 targetPos;
		OrbitAngles.yaw = 135;

		Camera.Position += Vector3.Up * (pawn.CollisionBounds.Center.z * pawn.Scale);
		Camera.Rotation = Rotation.From( OrbitAngles );

		targetPos = Camera.Position + Camera.Rotation.Backward * OrbitDistance;

		Camera.Position = targetPos;

		Camera.FieldOfView = 70f;
		Camera.FirstPersonViewer = null;
		Camera.Main.Ortho = true;
		Camera.Main.OrthoHeight = 500;

		Sound.Listener = new()
		{
			Position = pawn.AimRay.Position,
			Rotation = pawn.EyeRotation
		};
	}

	public void BuildInput()
	{
		if ( Input.Down( InputButton.PrimaryAttack ) )
		{
			var direction = Screen.GetDirection( Mouse.Position, Camera.FieldOfView, Camera.Rotation, Screen.Size );
			var hitPos = IntersectPlane( Camera.Position, direction, Entity.EyePosition.z );

			Entity.ViewAngles = (hitPos - Entity.EyePosition).EulerAngles;
		}

		OrbitAngles.pitch = OrbitAngles.pitch.Clamp( PitchClamp.x, PitchClamp.y );

		Entity.InputDirection = Input.AnalogMove;
	}
}
