# Moon Rabbit Rush 프로젝트 가이드

## 1. 기준 환경

- Unity `6000.5.4f1`
- 2D URP
- C# 네임스페이스 루트: `MoonRabbitRush`
- 게임 에셋 루트: `Assets`

Unity 에디터와 IDE가 자동 생성하는 `.meta`, `.csproj`, 솔루션 파일을 수동 편집하지
않는다. 에셋 이동과 이름 변경은 참조 보존을 위해 Unity Project 창에서 수행한다.

## 2. 폴더 구조

```text
Assets
├─ Art
│  ├─ Animations
│  └─ Sprites
├─ Audio
│  ├─ BGM
│  └─ SFX
├─ Data
│  ├─ Characters
│  ├─ Enemies
│  ├─ Skills
│  └─ Waves
├─ Prefabs
│  ├─ Characters
│  ├─ Enemies
│  ├─ Environment
│  ├─ Projectiles
│  └─ UI
├─ Scenes
├─ Scripts
│  ├─ Runtime
│  │  ├─ Core
│  │  ├─ Combat
│  │  ├─ Enemies
│  │  ├─ Skills
│  │  └─ Waves
│  ├─ Editor
│  └─ Tests
│     ├─ EditMode
│     └─ PlayMode
└─ UI
   ├─ Fonts
   ├─ Icons
   └─ Layouts
```

외부 에셋은 `Assets/ThirdParty/<PackageName>`에 격리하며 원본을 직접 수정하지 않는다.
프로토타입 파일은 `Assets/Dev`에만 두고 병합 전에 제거하거나 정식 위치로
옮긴다.

## 3. 이름 규칙

| 대상 | 규칙 | 예시 |
|---|---|---|
| C# 타입/파일 | PascalCase, 파일명과 타입명 일치 | `PlayerHealth.cs` |
| 인터페이스 | `I` + PascalCase | `IDamageable` |
| private 필드 | `_camelCase` | `_moveSpeed` |
| 프로퍼티/메서드 | PascalCase | `CurrentHealth`, `TakeDamage()` |
| bool | 의미가 드러나는 긍정형 | `IsAlive`, `CanAttack` |
| 프리팹 | `PF_<분류>_<이름>` | `PF_Enemy_Charger` |
| ScriptableObject 에셋 | `SO_<분류>_<이름>` | `SO_Enemy_Charger` |
| 스프라이트 | `SPR_<분류>_<이름>` | `SPR_Player_Idle_00` |
| 애니메이션 클립 | `ANIM_<대상>_<동작>` | `ANIM_Rabbit_Run` |
| 애니메이터 컨트롤러 | `AC_<대상>` | `AC_Rabbit` |
| 오디오 | `BGM_<이름>`, `SFX_<이름>` | `SFX_PlayerHit` |
| UI 오브젝트 | 역할 접두사 | `Btn_Start`, `Txt_Level`, `Img_HP` |
| 레이어 | PascalCase 단수형 | `Player`, `Enemy`, `Projectile` |

약어를 남용하지 않고 `Manager`는 실제로 여러 객체의 생명주기나 흐름을 조정할 때만
사용한다. 한 파일에는 원칙적으로 public 타입 하나만 둔다.

## 4. 씬 규칙

정식 씬은 아래 세 개를 기준으로 한다.

1. `00_Bootstrap`: 영속 서비스 초기화 후 타이틀로 이동한다.
2. `01_Title`: 타이틀과 캐릭터 선택을 담당한다.
3. `02_Game`: 전투 월드와 HUD를 포함한다.

빌드 순서는 반드시 위 순서를 유지한다. `Bootstrap`의 영속 객체만
`DontDestroyOnLoad`를 사용하며, 씬 전용 객체는 씬 종료 시 함께 제거한다.

각 씬의 루트 오브젝트 순서는 다음과 같이 통일한다.

```text
_Systems
_World
_UI
_Debug
```

씬 간 직접 참조를 만들지 않는다. 전환 데이터는 런타임 세션 컨텍스트를 통해
전달하고, 씬 로드는 하나의 씬 전환 서비스만 수행한다. 개발용 씬은
`Assets/Dev/Scenes`에 두며 빌드 목록에 포함하지 않는다.

## 5. ScriptableObject 사용 범위

ScriptableObject는 **변하지 않는 설계 데이터**에 사용한다.

- 캐릭터 기본 능력치, 시작 무기, 표시 정보
- 적 기본 능력치, 행동 파라미터, 드롭 데이터
- 무기/스킬 레벨별 수치, 아이콘, 설명, 희귀도
- 웨이브별 시간, 스폰 조합, 보스 출현 조건
- 오디오/이펙트 카탈로그

현재 HP, 경험치, 쿨다운, 웨이브 진행도처럼 플레이 중 바뀌는 값은 일반 런타임
객체나 컴포넌트에 둔다. ScriptableObject 원본을 런타임에 수정하지 않으며, 필요한
경우 값을 복사해 런타임 상태를 만든다. 시스템 간 이벤트 채널이나 전역 서비스
로케이터 용도로 ScriptableObject를 사용하지 않는다.

## 6. 시스템 계약 방향

`Scripts/Runtime`은 `MoonRabbitRush.Runtime` 어셈블리로 분리한다. 시스템 간에는
구현 컴포넌트 대신 아래의 작은 계약을 참조한다.

- 전투: `IDamageable`이 `DamageInfo`를 수신한다. 공격자는 대상 구현을 알지 않는다.
- 적: `IEnemy`가 활성/비활성 생명주기와 현재 상태를 노출한다.
- 스킬: `ISkill`이 준비 여부와 실행 계약을 제공한다. 수치 데이터와 실행 로직을
  분리한다.
- 웨이브: `IWaveDirector`가 진행 상태와 시작/정지 계약을 제공한다. 실제 스폰은
  별도 스포너에 위임한다.

인터페이스는 여러 구현 또는 테스트 대역이 필요한 경계에만 추가한다. UI는 상태를
직접 변경하지 않고 공개 명령을 호출하거나 읽기 전용 상태를 구독한다. 시스템 간
검색을 위한 `Find*` 호출과 전역 static 상태는 금지하며, Inspector 참조 또는
초기화 시 명시적 의존성 주입을 사용한다.

## 7. 공통 개발 규칙

- MonoBehaviour는 Unity 생명주기와 뷰 연결에 집중하고 계산 로직은 순수 C#으로 분리한다.
- `Update`에서 `Find`, LINQ, 문자열 조합, 반복 할당을 피한다.
- 인스펙터 필드는 `[SerializeField] private`를 기본으로 하고 public 필드는 금지한다.
- 구독한 이벤트는 `OnDisable` 또는 소유 객체의 종료 시점에 반드시 해제한다.
- 시간 기반 로직은 프레임 수 대신 `deltaTime`을 사용한다.
- 프리팹과 데이터 참조 누락은 조용히 무시하지 말고 개발 빌드에서 즉시 드러낸다.

## 8. 완료 기준

- 콘솔 컴파일 오류와 신규 경고가 없다.
- 플레이 모드 진입/종료 시 예외가 없다.
- 관련 데이터와 프리팹의 누락 참조가 없다.
- 변경된 동작을 수동 또는 자동 테스트로 검증했다.
- 해당 번호의 인수 조건과 문서를 함께 갱신했다.
