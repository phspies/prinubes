import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { ComputePlatformComponent } from './computeplatforms.component';
describe('MainComponent', () => {
  let component: ComputePlatformComponent;
  let fixture: ComponentFixture<ComputePlatformComponent>;
  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ComputePlatformComponent]
    }).compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(ComputePlatformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
