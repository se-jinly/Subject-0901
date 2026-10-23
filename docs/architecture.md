# 코드 구조 (2026-10-22 기준, 구조가 바뀌면 갱신)

## 씬 흐름
- Title: TitleManager.StartGame() → "Battle" 씬 로드. OpenSettings()로 설정창 열기
- Battle: WaveManager가 게임 전체 흐름을 진행. Esc → PauseManager 일시정지 메뉴
- 클리어: ClearUI.Restart() → Battle 재시작 / GoTitle() → Title
- 플레이어 사망: Player.Die() → 사망 이펙트(_deathEffectPrefab, HolyJudgment) 생성 → _dieWait 뒤 씬 재로드 (게임오버 화면 없음)

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

## 시스템 (System)
- TimeControl (static): 시간 멈춤을 이유별 플래그(HitStop, Reward, Pause)로 관리. 하나라도 켜져 있으면 timeScale 0, 씬 로드 시 전부 초기화. timeScale은 여기서만 바꿈
- PauseManager: Esc로 일시정지 메뉴(계속하기/설정/다시 시작/타이틀로/게임 종료). 다시 시작·타이틀로는 ClearUI 재사용. 입력 잠김(보상 선택·클리어)이나 사망 중에는 Esc 무시. 설정창이 열려 있으면 Esc는 설정창만 닫음
- GameSettings (static): 전체·배경음·효과음 볼륨, 전체화면, 화면 흔들림 설정. PlayerPrefs에 저장하고 게임 시작 시 자동 적용, 바뀌면 Changed 이벤트
  - 효과음 = AudioListener.volume(전체 × 효과음). Enemy의 PlayClipAtPoint 소리도 여기에 적용됨
  - BGM = BgmVolume 컴포넌트가 AudioSource에 (원래 볼륨 × 전체 × 배경음)을 적용. BGM 소스는 ignoreListenerVolume을 켬. 새 BGM 소스에는 BgmVolume을 붙일 것

## 전투 (Combat)
- IDamageable.TakeDamage(int, Vector3 넉백방향): Player, Enemy가 구현
- MeleeAttack: 앞쪽 OverlapSphere로 _targetLayer 타격 → HitFeedback(흔들림·히트스톱) → 효과음. 선택적으로 휘두름 이펙트(_swingEffectPrefab, 공격자 앞)와 타격 이펙트(_hitEffectPrefab, 맞은 대상 위치)를 생성. 비워두면 없음. 플레이어는 BloodSlash/BloodHit(핏빛), 적은 프리팹만 바꿔 다른 색을 쓸 예정
- RangedAttack: Projectile 생성 → Projectile이 레이어 비트마스크로 대상/벽 판정. _pierce를 켜면 대상을 관통(벽에서만 사라짐). _hitEffectPrefab을 지정하면 대상/벽에 닿은 위치에 이펙트 생성 (비우면 없음)
  - 화살 프리팹: Arrow는 플레이어용(이펙트 없음), EnemyArrow는 Arrow의 변형으로 적 병사·부대장용(빛 꼬리 + HolyHit), GiantArrow는 부대장 거대 화살
- 이펙트 색 방향: 플레이어 이펙트(BloodSlash, BloodHit, BloodSlashArc, BloodBurst, BloodWave)는 핏빛/악마적, 적 이펙트(HolySlash, HolyHit, EnemyArrow, GiantArrow, SlashArc)는 은/흰색 + 옅은 금색. 적 사망(DeathMark)은 플레이어 쪽 느낌이라 검붉은 색 유지
- HitEffect (싱글턴 Instance): 카메라 흔들림 + 히트스톱(TimeControl의 HitStop 플래그). 흔들림 끄기 설정이면 CameraFollowing.Shake()가 무시함
- 부대장 스킬 (Enemy에 붙이는 컴포넌트, 쿨타임 + 선딜 후 발동, 스킬 중 Enemy.IsActing = true로 일반 행동 정지)
  - LeaderDashSkill: 선딜 뒤 플레이어 쪽으로 돌진, 닿으면 피해 (근접 대장)
  - LeaderSlashSkill: 선딜 뒤 정면 부채꼴 베기 + SlashArc 이펙트 (근접 대장). 돌진 스킬도 있으면 Awake에서 둘 중 하나만 랜덤으로 켜짐
  - LeaderArrowSkill: 멈춰서 조준 후 거대 관통 화살(GiantArrow) 발사 (원거리 대장)
  - SlashSkill, BloodSkill: 플레이어 스킬 (E, Q)
- HitFlash: 맞으면 머티리얼 색을 잠깐 바꿈 (_BaseColor). Blink(선딜 시간)은 공격 직전 예고로 몸을 점점 빠르게 번쩍임 (보스 패턴, 부대장 스킬 선딜에서 호출). 중간에 맞으면 Play()가 끊고 원래 색으로 복구

## 캐릭터 (Character)
- Player: CharacterController 이동, 마우스 방향 회전, 대시(전투 중)/점프(비전투), 근접·원거리 공격, 체력과 보상 적용 메서드(AddMaxHealth, Heal, AddMoveSpeed, AddAttackDamage, MultiplyDashCooldown). InputLocked로 입력 잠금
- Enemy (abstract): Update에서 넉백 → TryAttack() → Chase()(부대 소속이면 FollowSlot). IsActing이 true면 이 흐름을 건너뜀. Target으로 추적 대상을 읽음. 사망 시 효과음 후 Destroy
- Squad: 근접·원거리 병사 대형 부대. 근접/원거리 중 랜덤 한 명이 부대장(MeleeLeader/RangedLeader 프리팹, 색이 다름)
  - MeleeEnemy: 추적 + 사거리 안이면 MeleeAttack
  - RangedEnemy: 거리 유지(가까우면 물러남) + RangedAttack
  - BossEnemy: 평소엔 근접 공격, 일정 시간마다 패턴(돌진 또는 소환) 코루틴. BossHealthBar 갱신. 이펙트 칸: _windupEffect(패턴 시작 전 모이는 빛), _dashTrailEffect(돌진 중 보스를 따라다님), _dashImpactEffect(돌진이 닿은 위치), _summonEffect(졸개 소환 위치마다), _dashWarningEffect(돌진 길 예고선). 비우면 없음. 돌진 방향은 선딜(예고) 시작 때 확정되어 그 방향으로 돌진하고, 선딜 동안 HitFlash.Blink로 몸이 깜빡임

## UI
- HealthBar: 플레이어 체력을 하트(Heart)로 표시, 하트 1칸 = 체력 2
- BossHealthBar (싱글턴 Instance): 보스 체력바, Show/Hide
- ClearUI: 클리어 화면 버튼 (Restart/GoTitle은 일시정지 메뉴에서도 재사용)
- SettingsPanel (프리팹 SettingsPanel): 볼륨 슬라이더 3개, 전체화면·화면 흔들림 끄기 토글, 뒤로 버튼. Battle과 Title이 같은 프리팹을 씀. Open()/Close()/IsOpen

## Util
- CameraFollowing: 플레이어 추적, 픽셀 스냅(RenderTexture + RawImage), Shake(), ScreenPointToRay()(플레이어 조준용)
- ViewCollider.cs: 콜라이더 기즈모 표시 (에디터 확인용)

## 입력과 애니메이션
- Input 액션: Move, Sprint(대시/점프), Attack, RangedAttack
- Animator 파라미터: Speed, IsGround, IsDash, Attack, Throw, Die

## 주의할 점
- Time.timeScale은 직접 바꾸지 말고 TimeControl.Set(이유, 켬/끔)으로만 바꿀 것. 새 멈춤 이유가 필요하면 TimeStopReason에 추가
- 전체화면 전환은 에디터 Game 뷰에서는 확인되지 않음 (빌드에서 확인)
- 싱글턴: HitEffect.Instance, BossHealthBar.Instance (씬에 하나씩 있어야 함)
- ViewCollider.cs 안의 클래스 이름이 ColliderGizmos라 파일 이름과 다름 (유니티에서 컴포넌트로 붙일 때 문제될 수 있음)
- 게임 상태를 총괄하는 GameManager는 아직 없음
