import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { NetworkPlatformComponent } from './networkplatforms.component';
describe('MainComponent', () => {
  let component: NetworkPlatformComponent;
  let fixture: ComponentFixture<NetworkPlatformComponent>;
  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [NetworkPlatformComponent]
    }).compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(NetworkPlatformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
