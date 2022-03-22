import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { CredentialComponent } from './credentials.component';
describe('MainComponent', () => {
  let component: CredentialComponent;
  let fixture: ComponentFixture<CredentialComponent>;
  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [CredentialComponent]
    }).compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(CredentialComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
