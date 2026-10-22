# 프로젝트 규칙 (Claude용)

## 기본

* 항상 한국어로 답할 것. 답변은 간결하게.
* 작업을 시작할 때 docs/progress.md를 먼저 읽을 것.
* 코드 구조는 docs/architecture.md, 게임 방향은 docs/game-design.md를 참고할 것.
* 유니티 버전: Unity 6000.3.21f1

## 작업 방식

* 기능은 한 번에 하나씩. 여러 파일을 바꾸는 작업은 먼저 계획만 보여주고, 승인 후 진행.
* 새 기능은 비슷한 기존 스크립트를 먼저 찾아 그 구조를 따르고, 계획에 "참고: 파일명"을 적을 것.
* 요청과 관련 없는 파일은 수정하지 말 것.
* 스크립트를 고친 뒤에는 /check 절차로 확인할 것.
* 기능 하나가 끝나면 /done 절차를 따를 것.
* 파일 3개 이상을 바꾸거나 기존 구조를 바꾸는 작업은, 계획에 파일별로 "무엇을 / 왜 / 기존 동작에 미치는 영향"을 설명할 것. 이때는 간결하게 규칙보다 이 설명을 우선.

## 유니티

* .unity, .prefab 파일은 직접 열거나 편집하지 말고 Unity MCP로 확인·수정.
* 플레이 모드 중에는 씬을 수정하지 말 것.
* Obsolete(사용 중단) API는 쓰지 말 것.
* 콘솔은 에러와 경고만 확인할 것. 디버그용으로 추가한 Debug.Log는 작업이 끝나면 지울 것.

## 코드 스타일 (기존 코드를 따를 것)

* 적은 Enemy를 상속하고 Awake / Chase / TryAttack / TakeDamage를 override.
* private 필드는 \_camelCase, 인스펙터 노출은 \[SerializeField] private.
* \[Header], \[Tooltip]은 한국어로.
* 비슷한 기능이 이미 있으면 새로 만들지 말고 기존 코드를 재사용·확장.
* 기존 주석(질문 형태의 학습 메모 포함)은 요청 없이 지우거나 고치지 말 것.

## 폴더

* 스크립트: Assets/Scripts/ 아래 Character, Combat, System, UI, Util
* 새 에셋(그림, 사운드 등)을 추가하면 docs/ai-usage.md에 출처를 기록.

