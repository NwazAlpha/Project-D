
# Puzzle Game Development TODO List

## 확정 규칙 요약
- 클리어 조건: 제거 가능한 모든 숫자 타일 제거
- 선택 방식: 축 정렬 사각형
- 타일 종류
  - 숫자 타일: 1 ~ Target-1 (제거 대상)
  - 0 타일: 합산에는 포함(0), 제거되지 않음, 선택 실패 조건 아님
  - 무한 타일(Blocker): 선택 가능하지만 포함 시 즉시 실패, 제거 불가, 타입 B 기준

---

## 1. 데이터 구조 / 프리셋

### StagePreset (ScriptableObject)
- [x] Width / Height
- [x] TargetNumber
- [x] Tile 데이터 구조
  - TileKind: Number / Zero / Blocker / Empty(Editor)
  - Value 또는 BlockerTypeId
- [x] BlockerType 정의 (여러 타입 확장 가능 구조)
- [x] 유효성 검증
  - 숫자 범위 체크
  - BlockerTypeId 유효성
  - Target > 0

---

## 2. 인게임 보드 시스템

### BoardModel
- [x] Tile 상태 관리 (Kind, Value, Removed)
- [x] 제거 가능한 숫자 타일 카운트 추적
- [x] 0 타일은 Removed 불가로 고정

### BoardView
- [x] 그리드 렌더링
- [x] 숫자/0/무한 타일 시각 구분
- [x] 제거 애니메이션 훅 (숫자 타일만)

---

## 3. 입력 / 선택 로직

### 드래그 선택
- [x] 터치 Down / Move / Up 처리
- [x] 시작/끝 셀 기반 축 정렬 사각형 계산
- [x] 선택 영역 하이라이트

### 선택 영역 해석
- [x] 선택된 숫자 타일 리스트
- [x] 선택된 0 타일 리스트
- [x] Blocker 포함 여부 플래그

---

## 4. 정답 판정 로직

### 실패 조건 (우선순위)
1. [x] 숫자 타일 개수 < 2
2. [x] Blocker 포함
3. [x] 숫자 타일 합 != Target

### 성공 처리
- [x] 선택 영역 내 숫자 타일 제거
- [x] 0 타일은 유지
- [x] 점수 증가
- [x] 선택 영역 리셋

---

## 5. 클리어 조건
- [x] 제거 가능한 숫자 타일 수 == 0
- [x] 클리어 UI 표시
- [x] 스테이지 클리어 저장 및 다음 스테이지 해금

---

## 6. 아웃게임 흐름

### Start
- [x] Tap anywhere → Lobby
- [x] ID / Password 더미 UI

### Lobby
- [x] Stage List 버튼
- [x] Shop 버튼 (비활성)
- [x] Settings 버튼

### Stage List
- [x] 스테이지 나열
- [x] 이전 스테이지 클리어 시 해금
- [x] 스테이지 선택 → InGame

---

## 7. Pause 메뉴
- [x] Pause 버튼
- [x] Resume
- [x] Settings
- [x] Exit → Stage List
- [x] Pause 중 입력 차단

---

## 8. 에디터 툴 (Stage Preset Editor)

### 기본 편집
- [x] 그리드 리사이즈
- [x] 숫자 브러쉬 (1 ~ Target-1)
- [x] 0 타일 브러쉬
- [x] Blocker 브러쉬 (타입 선택)
- [x] 지우개 (Empty)
- [x] Undo / Redo

### 자동 채우기
- [x] Empty 셀만 채움
- [x] 0 / 숫자 타일 규칙 반영
- [x] Blocker 포함 영역은 제거 후보에서 제외
- [x] 클리어 가능 시뮬레이터
  - 제거 가능한 사각형 반복 탐색
- [x] 실패 시 사유 로그 출력

---

## 9. 디버그 / QA
- [x] 선택 영역 합/타일 수 오버레이
- [x] 스테이지 검증 리포트
- [x] 자동 채우기 시드 저장
