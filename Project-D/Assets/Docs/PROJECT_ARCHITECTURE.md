# Unity 프로젝트 아키텍처 가이드

이 문서는 Unity 프로젝트의 설계, 핵심 구조, 폴더 관리 방식을 정리한 범용 가이드입니다.

> 💡 이 아키텍처는 중대형 규모의 Unity 게임 프로젝트에 적합합니다.

---

## 📁 프로젝트 루트 구조

```
ProjectRoot/
├── Assets/                          # Unity 에셋 루트
│   ├── [GameName]/                 # 🎮 메인 게임 코드베이스
│   ├── Plugins/                    # 네이티브 플러그인
│   ├── Resources/                  # 런타임 로드 리소스
│   ├── StreamingAssets/            # 스트리밍 에셋
│   └── ThirdParty/                 # 외부 에셋 및 SDK
├── Packages/                        # Unity 패키지
├── ProjectSettings/                 # Unity 프로젝트 설정
└── README.md                        # 프로젝트 문서
```

### 폴더 설명

| 폴더 | 용도 | 주의사항 |
|------|------|----------|
| `Assets/[GameName]/` | 게임 핵심 코드 및 에셋 | 프로젝트별 이름 사용 |
| `Assets/Plugins/` | 네이티브 플러그인 (.dll, .so) | 플랫폼별 분리 |
| `Assets/Resources/` | `Resources.Load()` 대상 | 남용 시 빌드 크기 증가 |
| `Assets/ThirdParty/` | 외부 에셋스토어 패키지 | 수정 지양 |
| `Library/`, `Temp/`, `obj/` | Unity 자동 생성 | 수동 편집 금지 |

---

## 🎮 메인 게임 코드베이스 구조

메인 게임 코드는 `Assets/[GameName]/` 디렉토리에 위치합니다.

```
[GameName]/
├── Scripts/                         # 📜 스크립트 코드
├── Prefabs/                         # 프리팹 (게임 오브젝트 템플릿)
├── Scenes/                          # 씬 파일
├── ArtWork/                         # 아트 리소스 (텍스처, 스프라이트 등)
├── ScriptableObject/                # ScriptableObject 에셋
├── Resources/                       # 런타임 로드용 리소스
├── Docs/                            # 📚 프로젝트 문서
└── Legacy/                          # 레거시/폐기 예정 에셋
```

---

## 📜 Scripts 폴더 아키텍처

스크립트 폴더는 **계층형 아키텍처(Layered Architecture)** 를 따릅니다.

```
Scripts/
├── _Core/                           # 🔧 핵심 데이터 및 열거형
├── _Module/                         # 🧩 재사용 가능한 모듈
├── Domain/                          # 🏢 도메인 로직 (비즈니스 로직)
├── Foundation/                      # 🏗️ 기반 시스템 (서비스, 매니저)
├── Presentation/                    # 🎨 프레젠테이션 레이어
└── Editor/                          # 🛠️ Unity 에디터 확장
```

### 레이어 의존성 규칙

```
┌─────────────────────────────────┐
│         Presentation            │  ← UI, 화면 표시
├─────────────────────────────────┤
│            Domain               │  ← 게임 로직, 엔티티
├─────────────────────────────────┤
│          Foundation             │  ← 서비스, 매니저, 인프라
├─────────────────────────────────┤
│    _Core / _Module              │  ← 공용 데이터, 유틸리티
└─────────────────────────────────┘

✅ 의존성 방향: 위 → 아래 (상위 레이어는 하위 레이어만 참조)
❌ 금지: 하위 레이어가 상위 레이어 참조
```

---

## 🔧 _Core - 핵심 데이터

게임 전반에서 사용되는 상수, 열거형, 데이터 객체를 정의합니다.

```
_Core/
├── Base/                            # 기본 추상 클래스
├── Enum/                            # 열거형 정의
│   ├── GameEnums.cs                # 게임 공통 열거형
│   ├── UIEnums.cs                  # UI 관련 열거형
│   └── [Feature]Enums.cs           # 기능별 열거형
├── DataObject/                      # 데이터 전송 객체 (DTO)
├── ScriptableObject/                # ScriptableObject 정의
├── Struct/                          # 구조체 정의
├── Constants.cs                     # 📌 전역 상수 (매직 스트링 중앙 관리)
└── BuildConfig.cs                   # 빌드 설정
```

### 상수 관리 예시

```csharp
// Constants.cs - 마법 문자열(Magic String) 중앙 관리
public static class Constants
{
    public static class AnimatorKey
    {
        public const string IsWalking = "IsWalking";
        public const string Speed = "Speed";
    }
    
    public static class PoolKey
    {
        public const string Bullet = "Pool_Bullet";
        public const string Effect = "Pool_Effect";
    }
    
    public static class PlayerPrefsKey
    {
        public const string SoundVolume = "SoundVolume";
        public const string Language = "Language";
    }
}
```

---

## 🧩 _Module - 재사용 모듈

프로젝트 전반 또는 여러 프로젝트에서 재사용되는 독립적인 모듈들입니다.

```
_Module/
├── Pooling/                         # 오브젝트 풀링 시스템
├── FSM/                             # 유한 상태 머신
├── UIAnimation/                     # UI 애니메이션
├── EventSystem/                     # 이벤트 버스
├── SaveLoad/                        # 세이브/로드 시스템
├── Singleton/                       # 싱글톤 베이스
├── Tools/                           # 유틸리티 도구
├── UI/                              # UI 공통 컴포넌트
├── ResultWrapper/                   # 결과 래핑 (성공/실패 패턴)
├── Exception/                       # 커스텀 예외
├── LogHelper.cs                     # 📌 로깅 헬퍼
├── MathHelper.cs                    # 수학 유틸리티
└── Extensions/                      # 확장 메서드
```

### 로깅 가이드라인

```csharp
// LogHelper 사용 예시
public static class LogHelper
{
    public static void Log(string message) 
        => Debug.Log($"[Game] {message}");
    
    public static void LogWarning(string message) 
        => Debug.LogWarning($"[Game] {message}");
    
    public static void LogError(string message) 
        => Debug.LogError($"[Game] {message}");
}
```

> ⚠️ **로깅 규칙**: 로그는 프로젝트 전용 `LogHelper`를 통해 남깁니다.

---

## 🏢 Domain - 도메인 로직

게임의 핵심 비즈니스 로직이 위치합니다. **Entity-Manager** 패턴을 사용합니다.

```
Domain/
├── [Feature]/                       # 🎯 기능 단위로 분리
│   ├── Entity/                     # 게임 엔티티 (개별 객체)
│   ├── Manager/                    # 도메인 매니저 (엔티티 관리)
│   ├── Config/                     # 기능별 설정
│   ├── DataObject/                 # 기능별 데이터 객체
│   └── State/                      # 상태 머신
├── UI/                              # 🖼️ UI 도메인 (MVP 패턴)
└── Common/                          # 도메인 공통
```

### Entity-Manager 패턴

```
┌──────────────────────────────────────────────────────┐
│                    Manager                            │
│  (엔티티 생성/삭제, 생명주기 관리, 쿼리)                   │
└──────────────────────────────────────────────────────┘
                          │
          ┌───────────────┼───────────────┐
          ▼               ▼               ▼
    ┌──────────┐   ┌──────────┐   ┌──────────┐
    │ Entity A │   │ Entity B │   │ Entity C │
    │ (개별 상태)│   │ (개별 상태)│   │ (개별 상태)│
    └──────────┘   └──────────┘   └──────────┘
```

### 예시 구조

```
Domain/
├── Player/
│   ├── Entity/PlayerEntity.cs       # 플레이어 개별 인스턴스
│   ├── Manager/PlayerManager.cs     # 플레이어 관리
│   └── State/PlayerState.cs         # 플레이어 상태
├── Enemy/
│   ├── Entity/EnemyEntity.cs
│   ├── Manager/EnemyManager.cs
│   └── State/EnemyState.cs
├── Inventory/
│   ├── Entity/ItemEntity.cs
│   └── Manager/InventoryManager.cs
└── UI/
    ├── HomeUI/
    ├── InventoryUI/
    └── SettingsUI/
```

### UI - MVP 패턴

UI는 **Model-View-Presenter (MVP)** 패턴을 따릅니다.

```
UI/[UIName]/
├── [UIName]Model.cs                 # 데이터 및 상태
├── [UIName]View.cs                  # Unity UI 컴포넌트 참조
└── [UIName]Presenter.cs             # 로직 및 View 업데이트
```

```
┌─────────────┐         ┌─────────────┐
│    View     │◄───────►│  Presenter  │
│ (UI 컴포넌트)│         │   (로직)     │
└─────────────┘         └──────┬──────┘
                               │
                               ▼
                        ┌─────────────┐
                        │    Model    │
                        │   (데이터)   │
                        └─────────────┘
```

### MVP 예시

```csharp
// InventoryUIModel.cs
public class InventoryUIModel
{
    public List<ItemData> Items { get; set; }
    public int SelectedIndex { get; set; }
}

// InventoryUIView.cs
public class InventoryUIView : MonoBehaviour
{
    [SerializeField] private Transform _itemContainer;
    [SerializeField] private Button _closeButton;
    
    public Transform ItemContainer => _itemContainer;
    public Button CloseButton => _closeButton;
}

// InventoryUIPresenter.cs
public class InventoryUIPresenter
{
    private InventoryUIModel _model;
    private InventoryUIView _view;
    
    public void Initialize(InventoryUIModel model, InventoryUIView view)
    {
        _model = model;
        _view = view;
        _view.CloseButton.onClick.AddListener(OnCloseClicked);
    }
    
    public void RefreshView() { /* View 업데이트 */ }
    private void OnCloseClicked() { /* 닫기 처리 */ }
}
```

---

## 🏗️ Foundation - 기반 시스템

게임의 인프라와 서비스 레이어입니다.

```
Foundation/
├── GameEntry/                       # 🚀 게임 진입점
│   └── GameEntry.cs                # 메인 엔트리 포인트
├── Service/                         # 서비스 레이어
│   ├── ResourceService.cs          # 리소스 로드
│   ├── AudioService.cs             # 오디오 관리
│   ├── SaveService.cs              # 저장 관리
│   └── ServiceLocator.cs           # 서비스 로케이터 패턴
├── Manager/                         # 시스템 매니저
│   ├── CameraManager.cs            # 카메라 관리
│   ├── SceneManager.cs             # 씬 관리
│   └── ManagerLocator.cs           # 매니저 로케이터
├── Interface/                       # 인터페이스 정의
├── Config/                          # 전역 설정
└── Debug/                           # 디버그 도구
```

### Service Locator 패턴

서비스는 `ServiceLocator`를 통해 접근합니다:

```csharp
// ServiceLocator.cs
public static class ServiceLocator
{
    private static Dictionary<Type, IService> _services = new();
    
    public static void Register<T>(T service) where T : IService
    {
        _services[typeof(T)] = service;
    }
    
    public static T Get<T>() where T : IService
    {
        return (T)_services[typeof(T)];
    }
}

// 사용 예시
var audioService = ServiceLocator.Get<AudioService>();
audioService.PlaySFX("click");
```

### GameEntry 구조

```csharp
// GameEntry.cs - 게임 초기화 진입점
public class GameEntry : MonoBehaviour
{
    private void Awake()
    {
        InitializeServices();
        InitializeManagers();
    }
    
    private void InitializeServices()
    {
        ServiceLocator.Register(new ResourceService());
        ServiceLocator.Register(new AudioService());
        ServiceLocator.Register(new SaveService());
    }
    
    private void InitializeManagers()
    {
        ManagerLocator.Register(new CameraManager());
        ManagerLocator.Register(new SceneManager());
    }
}
```

---

## 🎨 Presentation

프레젠테이션 레이어는 화면 표시와 관련된 로직을 담당합니다.

```
Presentation/
├── Camera/                          # 카메라 효과
├── VFX/                             # 비주얼 이펙트
├── Shader/                          # 셰이더
└── PostProcessing/                  # 포스트 프로세싱
```

---

## 🛠️ Editor

Unity 에디터 확장 스크립트입니다. **빌드에 포함되지 않습니다.**

```
Editor/
├── CustomInspector/                 # 커스텀 인스펙터
├── EditorWindow/                    # 에디터 윈도우
├── BuildPipeline/                   # 빌드 자동화
└── Tools/                           # 에디터 도구
```

---

## 🔑 핵심 설계 패턴

### 1. Entity-Manager 패턴
- **Entity**: 개별 게임 오브젝트의 상태와 행동
- **Manager**: 엔티티의 생명주기 및 상호작용 관리, 쿼리 제공

### 2. MVP 패턴 (UI)
- **Model**: 데이터 및 상태
- **View**: Unity UI 컴포넌트 참조 (로직 없음)
- **Presenter**: 로직 및 View 업데이트

### 3. Service Locator 패턴
- 서비스간 느슨한 결합
- 중앙화된 서비스 접근
- 테스트 용이성 향상

### 4. Partial Class 분리
- 대형 클래스는 기능별로 파티셜 클래스로 분리
- 예: `GameManager.cs`, `GameManager.Save.cs`, `GameManager.Audio.cs`

```csharp
// GameManager.cs
public partial class GameManager { /* 핵심 로직 */ }

// GameManager.Save.cs
public partial class GameManager { /* 저장 관련 */ }

// GameManager.Audio.cs
public partial class GameManager { /* 오디오 관련 */ }
```

### 5. ScriptableObject 기반 설정
- 게임 설정 값은 ScriptableObject로 관리
- Inspector에서 쉽게 조정 가능
- 버전 관리 친화적

```csharp
[CreateAssetMenu(fileName = "GameConfig", menuName = "Config/GameConfig")]
public class GameConfig : ScriptableObject
{
    [SerializeField] private float _playerSpeed = 5f;
    [SerializeField] private int _maxHealth = 100;
    
    public float PlayerSpeed => _playerSpeed;
    public int MaxHealth => _maxHealth;
}
```

---

## 📋 코딩 컨벤션

### 네이밍 규칙

| 항목 | 규칙 | 예시 |
|------|------|------|
| 클래스 | PascalCase | `PlayerManager` |
| 메서드 | PascalCase | `SpawnPlayer()` |
| private 필드 | _camelCase | `_playerCount` |
| public 프로퍼티 | PascalCase | `PlayerCount` |
| SerializeField | _camelCase | `[SerializeField] private int _maxCount;` |
| 열거형 | E + PascalCase | `EPlayerState` |
| 인터페이스 | I + PascalCase | `IEntity` |
| 상수 | PascalCase 또는 UPPER_CASE | `MaxHealth` 또는 `MAX_HEALTH` |

### 네임스페이스

폴더 구조를 반영합니다:
```csharp
namespace [ProjectName].Core.Enums { }
namespace [ProjectName].Domain.Player { }
namespace [ProjectName].Foundation.Service { }
```

### 주석 규칙

```csharp
/// <summary>
/// 플레이어를 스폰합니다.
/// </summary>
/// <param name="spawnPoint">스폰 위치</param>
/// <returns>생성된 플레이어 엔티티</returns>
public PlayerEntity SpawnPlayer(Vector3 spawnPoint) { }
```

### 코드 구조 순서

```csharp
public class ExampleClass : MonoBehaviour
{
    // 1. 상수
    private const float DEFAULT_SPEED = 5f;
    
    // 2. SerializeField
    [SerializeField] private float _speed;
    
    // 3. Private 필드
    private bool _isInitialized;
    
    // 4. Public 프로퍼티
    public bool IsInitialized => _isInitialized;
    
    // 5. Unity 라이프사이클
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    
    // 6. Public 메서드
    public void Initialize() { }
    
    // 7. Private 메서드
    private void DoSomething() { }
}
```

---

## 📂 폴더 생성 체크리스트

새 프로젝트 시작 시:

```
Assets/
└── [GameName]/
    ├── Scripts/
    │   ├── _Core/
    │   │   ├── Enum/
    │   │   ├── DataObject/
    │   │   └── Constants.cs
    │   ├── _Module/
    │   │   ├── Pooling/
    │   │   └── LogHelper.cs
    │   ├── Domain/
    │   │   └── UI/
    │   ├── Foundation/
    │   │   ├── GameEntry/
    │   │   └── Service/
    │   └── Editor/
    ├── Prefabs/
    ├── Scenes/
    ├── ScriptableObject/
    └── Docs/
```

---

## 🔒 빌드 제외 / 편집 금지 폴더

| 폴더 | 설명 | 주의 |
|------|------|------|
| `Library/` | Unity 캐시 | 수동 편집 금지 |
| `Temp/` | 임시 파일 | 수동 편집 금지 |
| `Logs/` | 로그 파일 | 수동 편집 금지 |
| `obj/` | 빌드 중간 파일 | 수동 편집 금지 |
| `.csproj` | Unity 자동 생성 | 수동 편집 지양 |

---

## 🚀 적용 가이드

### 1. 신규 프로젝트
1. 위 폴더 구조대로 디렉토리 생성
2. `Constants.cs`, `LogHelper.cs` 기본 파일 생성
3. `ServiceLocator.cs`, `ManagerLocator.cs` 설정
4. `GameEntry.cs` 진입점 구성

### 2. 기존 프로젝트 마이그레이션
1. `_Core/`, `_Module/` 폴더 먼저 생성
2. 열거형, 상수를 `_Core/`로 이동
3. 유틸리티 클래스를 `_Module/`로 이동
4. 도메인 로직을 `Domain/`으로 분리
5. 서비스/매니저를 `Foundation/`으로 분리

---

*마지막 업데이트: 2025-12-29*
