import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { LoadBalancerPlatformComponent } from './loadbalancerplatforms.component';
describe('MainComponent', () => {
  let component: LoadBalancerPlatformComponent;
  let fixture: ComponentFixture<LoadBalancerPlatformComponent>;
  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [LoadBalancerPlatformComponent]
    }).compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(LoadBalancerPlatformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
