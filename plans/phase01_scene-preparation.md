# Phase 01: 씬 준비 (rose-cli)

## 목표
- 씬에서 불필요한 오브젝트(pile, cannonball 인스턴스)를 제거한다.
- game GO에서 중복된 SimpleGameBase 컴포넌트를 제거한다.
- 카메라 위치/회전을 조정하여 전체 필드(shooter ~ pile 영역)가 보이게 한다.
- ground 스케일을 확대하여 shooter(-24.5)부터 pile 영역(~40)까지 커버한다.

## 선행 조건
- 없음 (최초 phase)
- 에디터가 실행 중이어야 한다: `dotnet run --project /home/alienspy/git/IronRose/src/IronRose.RoseEditor`

## 작업 (rose-cli 명령)

이 Phase는 코드 작성이 아닌 **에디터 씬 편집** 작업이다.
에디터를 실행한 상태에서 rose-cli 명령으로 수행한다.

### 1-1. 에디터 연결 확인
```
ping
```

### 1-2. 현재 씬 구조 확인
```
scene.tree
```
- 각 GO의 ID를 파악한다.
- 확인할 GO: `Main Camera`, `ground`, `Spot Light`, `pile`, `game`, `shooter`, `cannonball`

### 1-3. pile prefab 인스턴스 제거
```
go.destroy <pile_id>
```
- `scene.tree` 결과에서 `pile`의 ID를 사용

### 1-4. cannonball prefab 인스턴스 제거
```
go.destroy <cannonball_id>
```
- `scene.tree` 결과에서 `cannonball`의 ID를 사용

### 1-5. game GO에서 SimpleGameBase 컴포넌트 제거
```
component.remove <game_id> SimpleGameBase
```
- AngryClawdGame이 SimpleGameBase를 상속하므로 별도의 SimpleGameBase 컴포넌트가 있으면 Start/Update가 이중 호출된다.
- 먼저 `component.list <game_id>`로 확인 후, SimpleGameBase가 별도로 존재하면 제거.

### 1-6. ground 스케일 확대
```
transform.set_scale <ground_id> 10,1,10
```
- 현재 5x5 Plane (실제 50x50) -> 10x1x10 (실제 100x100)으로 확대
- shooter(-24.5)부터 pile 영역(~40)까지 충분히 커버

### 1-7. 카메라 위치/회전 조정
```
transform.set_position <cam_id> -12,8,-25
transform.set_rotation <cam_id> 15,0,0
```
- 전체 필드가 보이도록 위치 조정
- 시각적 확인 후 미세 조정 필요할 수 있음

### 1-8. 씬 저장
```
scene.save
```

### 1-9. 결과 확인
- 스크린샷으로 씬 상태 확인
- 씬에 남아있어야 할 GO: Main Camera, ground, Spot Light, game, shooter
- 제거되어야 할 GO: pile, cannonball

## 검증 기준
- [ ] `scene.tree` 결과에 pile, cannonball이 없음
- [ ] game GO에 AngryClawdGame만 존재 (SimpleGameBase 중복 없음)
- [ ] 카메라에서 shooter 위치와 pile 영역이 모두 보임
- [ ] ground가 전체 필드를 커버함
- [ ] 씬이 저장됨

## 참고
- 카메라 정확한 위치/회전은 시각적으로 확인하면서 조정해야 하므로, 위 값은 시작점이다.
- Phase 4(테스트/밸런싱)에서 다시 미세 조정할 수 있다.
