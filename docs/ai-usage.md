# AI 사용 기록

에셋이나 코드를 AI로 만들었을 때 한 줄씩 기록합니다. 스팀 신고서 작성이나 저작권 확인 때 씁니다.

| 날짜 | 파일 / 에셋 | 사용한 AI 도구 | AI가 한 일 | 내가 수정한 것 |
|---|---|---|---|---|
| (예) 2026-10-15 | Assets/Scripts/... | Claude Code | 초안 작성 | 수치 조정 |
| 2026-10-22 | Assets/Prefab/BloodBurst.prefab, Assets/Effects/BloodRed.mat | Claude Code | 핏빛 파티클 이펙트와 머티리얼을 코드로 생성 (외부 이미지 없음, 시험용) | - |
| 2026-10-22 | Assets/Prefab/SlashArc.prefab, Assets/Effects/SlashWhite.mat | Claude Code | 횡베기 부채꼴 파티클 이펙트와 머티리얼을 코드로 생성 (외부 이미지 없음, 시험용) | - |
| 2026-10-22 | Assets/Prefab/DeathMark.prefab, Assets/Effects/Disc.png, SoftDisc.png, DeathCircle.mat, DeathSmoke.mat | Claude Code | 적 사망 이펙트(검붉은 원 + 연기). 원 모양 텍스처 2장(Disc, SoftDisc)을 코드로 계산해서 생성 | - |
| 2026-10-22 | Assets/Prefab/BloodSlash.prefab, BloodHit.prefab (기존 BloodRed.mat, DeathSmoke.mat 재사용) | Claude Code | 플레이어 근접 공격의 핏빛 휘두름(머리가 호를 그려 나가고 꼬리부터 지워지는 잔상 + 흩어지는 네모)·타격(쐐기형 교차 베기 두 줄 + 흩어지는 네모) 파티클 이펙트를 코드로 생성 (외부 이미지 없음, 시험용) | - |
| 2026-10-22 | Assets/Prefab/BloodSlashArc.prefab (기존 BloodRed.mat 재사용) | Claude Code | 플레이어 E 베기용 핏빛 이펙트. 기존 SlashArc의 모양은 두고 색을 붉게 바꾼 사본에, 근접 공격식 쐐기 잔상과 흩어지는 네모를 더해 코드로 생성 (외부 이미지 없음, 시험용) | - |
| 2026-10-22 | Assets/Prefab/HolySlash.prefab, HolyHit.prefab, EnemyArrow.prefab, Assets/Effects/EnemyArrow.mat, HolyArrow.mat (기존 SlashWhite.mat 재사용) | Claude Code | 적 근접 휘두름·타격, 적 화살(Arrow 변형 + 빛 꼬리) 이펙트를 은/흰색 + 옅은 금색으로 코드로 생성. 머티리얼도 코드로 생성 (외부 이미지 없음, 시험용) | - |
| 2026-10-22 | Assets/Prefab/GiantArrow.prefab, SlashArc.prefab (기존 에셋 수정) | Claude Code | 거대 화살에 빛 꼬리·적중 이펙트·HolyArrow.mat 적용, 횡베기 SlashArc 색을 은/흰색 + 금색 파편으로 조정 | - |
| 2026-10-22 | Assets/Prefab/HolyJudgment.prefab (기존 SlashWhite.mat 재사용) | Claude Code | 플레이어 사망 이펙트(빛기둥 + 바닥 인장 링 + 충격 링 + 떠오르는 빛 조각, 은/흰색 + 금색)를 코드로 생성 (외부 이미지 없음, 시험용) | - |
| 2026-10-23 | Assets/Prefab/HolyWindup.prefab, HolyDashTrail.prefab, HolyDashImpact.prefab, HolySummon.prefab (기존 SlashWhite.mat 재사용) | Claude Code | 보스 돌진·소환 패턴 이펙트(모이는 빛, 돌진 꼬리, 충격, 소환 인장·빛기둥, 은/흰색 + 금색)를 코드로 생성 (외부 이미지 없음, 시험용) | - |
| 2026-10-23 | Assets/Prefab/HolyHit.prefab (기존 에셋 수정), DeathMark.prefab (속도 모듈 축 방식 통일) | Claude Code | HolyHit을 BloodHit과 같은 쐐기 스타일로 다시 칠함. DeathMark Smoke의 Velocity 모듈 X, Y, Z 방식을 통일해 콘솔 경고 제거 | - |
| 2026-10-23 | Assets/Prefab/HolyDashWarning.prefab (기존 SlashWhite.mat 재사용) | Claude Code | 보스 돌진 예고선(양쪽 가장자리 선, 진행 방향 화살표, 옅은 면, 도착 링, 깜빡임 펄스)을 코드로 생성 (외부 이미지 없음, 시험용) | - |

## 외부 에셋 출처
AI로 만든 것이 아닌 외부 에셋의 출처를 기록합니다.

| 날짜 | 에셋 | 출처 | 사용 위치 |
|---|---|---|---|
| 2026-10-22 | Assets/Sprites/16x16 Heart Health Red.png | https://redreeh.itch.io/pixelhearts-16x16 (redreeh) | HP 하트 UI (Heart.prefab). 라이선스는 출처 페이지에서 확인 필요 |

## 이전 작업 메모
- [이 문서를 만들기 전 작업 중 기억나는 것을 적기. 나중에 기억을 되살려 쓴 메모라는 점을 그대로 둘 것]
