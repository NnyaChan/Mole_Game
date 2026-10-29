# Mole_Game

B급 병맛 감성의 2D 채굴 게임. 매일 랜덤으로 생성되는 5층 광산을 파 내려가 광물을 캐고,
랜턴(=하루 시간)이 꺼지기 전에 지상으로 돌아와 광물을 팔고 장비를 사는 루프.

- 엔진: Unity 6000.5.9f1 (URP 2D)
- 입력: New Input System (`Assets/Settings/InputSystem_Actions.inputactions`, PlayerInput 이벤트 방식)
- 씬: `Assets/0. Scenes/SampleScene.unity` 하나 (하루가 끝나면 이 씬을 다시 로드)

## 기획 문서 (작업 전 관련 문서를 먼저 읽을 것)

| 문서 | 내용 |
| --- | --- |
| `Docs/00_Overview.md` | 컨셉, 핵심 루프, 하루 흐름 |
| `Docs/01_Player_Controls.md` | 조작, 이동, 벽타기, 조준 |
| `Docs/02_Mine_Map.md` | 층 구조, 랜덤 맵 생성, 층별 광물 개수 |
| `Docs/03_Blocks_Mining.md` | 블록 종류, 곡괭이 단계, 채굴 시간표 |
| `Docs/04_Lantern_Day.md` | 랜턴, 귀환, 암전 패널티, 구조 |
| `Docs/05_Economy.md` | 판매(까마귀), 정산, 인터넷 상점, 뉴스 시세 |
| `Docs/06_Items.md` | 영구 아이템, 소모 아이템, 인벤토리 |
| `Docs/07_UI.md` | HUD와 화면 목록 |
| `Docs/08_Balance.md` | 층별 기대 수입, 일차별 진행 목표 (약 15일) |
| `Docs/Roadmap.md` | 구현 현황, 다음 할 일, 결정이 필요한 항목 |

- 각 문서의 **"미정"** 표시는 아직 기획이 정해지지 않은 부분이다. 임의로 수치를 정하지 말고 사용자에게 먼저 물어볼 것.
- 기능을 구현하거나 기획이 바뀌면 `Docs/Roadmap.md`와 해당 문서를 함께 갱신할 것.

## 폴더 규칙

- 스크립트: `Assets/1. Scripts/<시스템>/` (Map, Mining, Lentern, UI, ScriptableObject). 매니저류는 `1. Scripts/` 바로 아래.
- 데이터 에셋(ScriptableObject): `Assets/3. Prefebs/ScriptableObject/` (`TestFloors.asset`, `TestBlockCatalog.asset`)
- 타일 에셋: `Assets/4. Meterials/Floor/...`
- 폴더명 오타(`Lentern`, `Prefebs`, `Meterials`)는 기존 그대로 둔다. 이름 변경은 사용자가 Unity 에디터에서 직접 한다.

## 아키텍처 요약

- `GameManager`: 싱글톤 + DontDestroyOnLoad. 상태(Playing/Ending), 일차(Day), 하루 종료 → 암전 → 씬 재로드.
- `RandomMapGenerator`: Floor 에셋을 읽어 Tilemap에 `SetTile`로 맵 생성. 오브젝트 위치가 맵 좌상단 기준점(`TopCellY`).
- `ZoneDetector`: 플레이어 깊이(Depth) 계산. 지상/광산 진입, 귀환 가능 구역 판정.
- `LanternManager` / `LanternUI`: 광산 안에서만 시간 감소, 0이 되면 `GameManager.EndDay`.
- `TileBreaker`(조준) → `MiningController`(누르고 있는 시간 계산, 파괴) → `BlockBreakEffect`(균열 애니메이션).
- `BlockCatalog`(SO): 타일→블록 종류 매핑 + 곡괭이 단계별 채굴 시간표.
- `PlayerManager`: 좌우 이동, 벽타기(Shift).
- 시스템 간 연결은 `event Action` + `FindAnyObjectByType`로 느슨하게 연결되어 있다. 이 방식을 따를 것.

## Unity 작업 방식

- Claude는 Unity 에디터를 직접 실행하거나 볼 수 없다. 스크립트 수정 후 사용자가 Unity로 돌아가 컴파일/플레이 테스트한다.
- 컴파일 에러나 런타임 로그 확인이 필요하면 Editor.log를 읽을 것:
  - Windows: `%LOCALAPPDATA%\Unity\Editor\Editor.log`
- 새 스크립트는 `Assets/` 아래에 만들어도 된다 (`.meta`는 Unity가 자동 생성). 단, 기존 파일의 **이름 변경·이동은 하지 말 것** (`.meta`가 따로 놀아 참조가 깨진다). 필요하면 사용자에게 Unity에서 하도록 안내.
- 스크립트 작성 후에는 사용자가 해야 할 에디터 작업(오브젝트에 컴포넌트 붙이기, 인스펙터 연결, PlayerInput 이벤트 연결 등)을 단계별로 정리해서 알려줄 것.
- `Library/`, `Temp/`, `Logs/`, `UserSettings/`는 Unity 자동 생성 폴더이므로 읽거나 수정하지 말 것.

## 코드 규칙

- 주석은 한국어. 클래스/메서드/프로퍼티 PascalCase, 필드 camelCase, 인스펙터 노출은 `[SerializeField]` private 우선.
- 맵 타일은 반드시 Tilemap `SetTile`로 처리. 블록을 개별 프리팹으로 만들지 말 것.
- 디버그 로그는 `[시스템명] 내용` 형식 (예: `[랜턴] 남은 칸: 3/10`).
- `.meta`, `.unity`, `.prefab`, `.asset` 파일은 직접 편집하지 말 것. 씬/인스펙터 연결이 필요하면 사용자에게 할 일을 안내할 것.
- `BlockType` enum 순서는 채굴 시간표의 열 순서이므로 바꾸지 말 것.
