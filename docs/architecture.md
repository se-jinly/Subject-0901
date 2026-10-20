# 코드 구조 (2026-10-20 기준, 구조가 바뀌면 갱신)

## 씬 흐름
- Title: TitleManager.StartGame() → "Battle" 씬 로드
- Battle: WaveManager가 게임 전체 흐름을 진행
- 클리어: ClearUI.Restart() → Battle 재시작 / GoTitle() → Title
- 플레이어 사망: Player.Die() → 1.5초 뒤 씬 재로드 (게임오버 화면 없음)

## 게임 루프 (WaveManager.WaveRoutine)
1. 웨이브 텍스트 갱신 → 대기(_waveInterval)
2. SpawnWave: EnemySpawner.Spawn()으로 근접/원거리/보스 생성 (NavMesh 위 랜덤 위치, SetTarget(플레이어))
3. 적이 모두 Destroy될 때까지 대기 (처치 수 집계)
4. 마지막 웨이브가 아니면 RewardManager.ShowAndWait()
5. 전부 끝나면 클리어 UI + 경과 시간/처치 수 표시, 입력 잠금

## 보상 (System)
- Reward: 보상 데이터 (RewardType, 제목, 설명, 값). RewardType = MaxHealth, Heal, MoveSpeed, AttackDamage, DashCooldown
- RewardManager: _pool에서 중복 없이 카드 수만큼 뽑아 RewardCard에 표시 → 선택 시 Apply()가 Player 메서드 호출
- 새 보상 추가: RewardType에 항목 추가 → RewardManager.Apply()에 case 추가 → 필요하면 Player에 메서드 추가

## 전투 (Combat)
- IDamageable.TakeDamage(int, Vector3 넉백방향): Player, Enemy가 구현
- MeleeAttack: 앞쪽 OverlapSphere로 _targetLayer 타격 → HitFeedback(흔들림·히트스톱) → 효과음
- RangedAttack: Projectile 생성 → Projectile이 레이어 비트마스크로 대상/벽 판정
- HitEffect (싱글턴 Instance): 카메라 흔들림 + 히트스톱(Time.timeScale = 0)
- HitFlash: 맞으면 머티리얼 색을 잠깐 바꿈 (_BaseColor)

## 캐릭터 (Character)
- Player: CharacterController 이동, 마우스 방향 회전, 대시(전투 중)/점프(비전투), 근접·원거리 공격, 체력과 보상 적용 메서드(AddMaxHealth, Heal, AddMoveSpeed, AddAttackDamage, MultiplyDashCooldown). InputLocked로 입력 잠금
- Enemy (abstract): Update에서 넉백 → TryAttack() → Chase(). 사망 시 효과음 후 Destroy
  - MeleeEnemy: 추적 + 사거리 안이면 MeleeAttack
  - RangedEnemy: 거리 유지(가까우면 물러남) + RangedAttack
  - BossEnemy: 평소엔 근접 공격, 일정 시간마다 패턴(돌진 또는 범위공격) 코루틴. BossHealthBar 갱신

## UI
- HealthBar: 플레이어 체력을 하트(Heart)로 표시, 하트 1칸 = 체력 2
- BossHealthBar (싱글턴 Instance): 보스 체력바, Show/Hide
- ClearUI: 클리어 화면 버튼

## Util
- CameraFollowing: 플레이어 추적, 픽셀 스냅(RenderTexture + RawImage), Shake(), ScreenPointToRay()(플레이어 조준용)
- ViewCollider.cs: 콜라이더 기즈모 표시 (에디터 확인용)

## 입력과 애니메이션
- Input 액션: Move, Sprint(대시/점프), Attack, RangedAttack
- Animator 파라미터: Speed, IsGround, IsDash, Attack, Throw, Die

## 주의할 점
- Time.timeScale을 바꾸는 곳이 3군데: HitEffect(히트스톱), RewardManager(보상 선택 중 0), Player.DieRoutine. 서로 겹치면 멈춤이 풀리거나 안 풀리는 버그가 생길 수 있음
- 싱글턴: HitEffect.Instance, BossHealthBar.Instance (씬에 하나씩 있어야 함)
- ViewCollider.cs 안의 클래스 이름이 ColliderGizmos라 파일 이름과 다름 (유니티에서 컴포넌트로 붙일 때 문제될 수 있음)
- 게임 상태를 총괄하는 GameManager는 아직 없음
