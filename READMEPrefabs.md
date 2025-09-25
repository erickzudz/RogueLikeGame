# RogueLikeGame – PUN2 (Top-Down)

Proyecto de prototipo top-down con **Photon PUN 2**: movimiento de jugador, disparo en red, enemigos con IA básica, stun, boss con ataques, y HUD móvil (joystick + botones).

## Estructura

Assets/
Resources/
Prefabs/
Player.prefab
Bullet.prefab
Enemy.prefab
EnemyBullet.prefab
Basher.prefab
NormalShoot.prefab
Sniper.prefab
SniperV2.prefab
Shotgun.prefab
Boss.prefab
Scripts/
Player/
PlayerMovement.cs
WeaponController.cs
MobileHUD.cs
MobileJoystick.cs
MobileButton.cs
Combat/
Health.cs
StunReceiver.cs // versión PUN, usa PhotonNetwork.Time
NetworkBullet.cs // bala PUN con OnTriggerEnter
Enemy/
Enemy.cs // IA general con tipos (Normal, NormalShoot, Kamikaze, Sniper, SniperV2, ShooterSpread, Basher)
BossController.cs // boss con cone, radial, laser lock, summon
Core/
TestPlayModeBootstrap.cs (spawner de escena / helpers)



## Capas / Tags recomendadas
- **Layers**: `Player`, `Enemy`, `Bullet`, `Floor`.
- **Tags**: `Player`, `Enemy`.
- **Physics → Layer Collision Matrix**:
  - `Bullet` colisiona con `Enemy` (balas del jugador).
  - `EnemyBullet` (si usas layer aparte) colisiona con `Player`.
  - Evitar `Bullet` vs `Bullet`.

## Prefabs y componentes clave

### Player
- `PhotonView`, `PhotonTransformView`, `CharacterController`
- `PlayerMovement.cs`: mueve con teclado o **MobileHUD** si existe (joystick).
- `WeaponController.cs`: instancia `Prefabs/Bullet` vía `PhotonNetwork.Instantiate` con datos (dirección, daño, velocidad, stun opcional).
- `Health.cs`: vida y método `TakeDamage(float)`.
- `StunReceiver.cs` (PUN): usa `PhotonNetwork.Time` para calcular “stunUntil”.

### Bullet (del jugador)
- `SphereCollider (isTrigger) + Rigidbody (isKinematic)`
- `PhotonView`, `PhotonTransformView`
- `NetworkBullet.cs`:
  - Lee `InstantiationData` (dir, daño, speed, stunSeconds).
  - `OnTriggerEnter` aplica `Health.TakeDamage` y opcional `StunReceiver.ApplyStun`.

### Enemy
- `Enemy.cs` con `EnemyType`:
  - `Normal` – camina.
  - `NormalShoot` – camina y dispara.
  - `Kamikaze` – acelera y se destruye al contacto.
  - `Sniper` – se para para disparar.
  - `SniperV2` – ráfaga de 3.
  - `ShooterSpread` – abanico.
  - `Basher` – balas que stunean al Player.
- Usa `PhotonNetwork.IsMasterClient` para IA (equivalente a “server authority”).
- Dispara `EnemyBullet` (o `BulletBasher` con `stunSeconds` > 0).

### Boss
- `BossController.cs` con ataques:
  - **Cone** (escopeta hacia el jugador).
  - **Radial** (círculo bullet hell).
  - **Laser lock** (raycast que “persigue” al jugador; daño por segundo).
  - **Summon** (invoca minions aleatorios de una lista).
- Toggle de ataques por Inspector.
- `Laser` es un `LineRenderer` (el color/ancho se editan en el componente).

## HUD Móvil
- `Canvas/ HUD` con:
  - `JoyBG` + `JoyHandle` (círculos) + `MobileJoystick.cs`.
  - `FireBtn` (Button) + `MobileButton.cs`.
  - `AbilityBtn` (Button) + `MobileButton.cs`.
- `MobileHUD.cs`: singleton con refs a joystick y botones. `WeaponController` lee `FireBtn.IsHeld`.

## PUN / Resources
- Todos los prefabs instanciados por PUN deben estar bajo `Assets/Resources/`.
- Se usan rutas tipo `"Prefabs/Bullet"`, `"Prefabs/Enemy"`, `"Prefabs/Boss"`.

## Cámara
- `FollowCameraPun.cs`: sigue al local player con `offset` y `LookAt`.

## Notas de colisión
- Balas: `SphereCollider` como **Trigger**, `Rigidbody` **isKinematic**.
- Hitbox del Player/Enemy: `CapsuleCollider` (NO trigger) o collider en hijo “Hitbox”.
