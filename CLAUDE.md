# 프로젝트 규칙 (Claude용)

## 기본
- 항상 한국어로 답할 것. 답변은 간결하게.
- 작업을 시작할 때 docs/progress.md를 먼저 읽을 것.
- 게임 방향은 docs/game-design.md를 참고할 것.
- 유니티 버전: [직접 적기, 예: Unity 6000.x]

## 작업 방식
- 기능은 한 번에 하나씩. 여러 파일을 바꾸는 작업은 먼저 계획만 보여주고, 승인 후 진행.
- 요청과 관련 없는 파일은 수정하지 말 것.
- 스크립트를 수정한 뒤에는 Unity MCP로 콘솔을 확인해 컴파일 에러가 없는지 확인할 것.
- 작업이 끝나면 바꾼 파일 목록과 변경 내용을 3줄 이내로 요약.
- 기능 하나가 끝나면 docs/progress.md를 최신 상태로 덮어쓰고(10줄 이내), 커밋할지 물어볼 것. 커밋 메시지는 한국어로.

## 유니티
- .unity, .prefab 파일은 직접 열거나 편집하지 말고 Unity MCP로 확인·수정.
- 플레이 모드 중에는 씬을 수정하지 말 것.
- Obsolete(사용 중단) API는 쓰지 말 것.

## 코드 스타일 (기존 코드를 따를 것)
- 적은 Enemy 기반 클래스를 상속하고 Awake / Chase / TryAttack / TakeDamage를 override.
- private 필드는 _camelCase, 인스펙터 노출은 [SerializeField] private.
- [Header], [Tooltip]은 한국어로.
- 비슷한 기능이 이미 있으면 새로 만들지 말고 기존 코드를 재사용·확장.

## 폴더
- 스크립트: Assets/Scripts/ 아래 Character, Combat, System, UI, Util
- 새 에셋(그림, 사운드 등)을 추가하면 docs/ai-usage.md에 출처를 기록.
