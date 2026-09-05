import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LeavePolicyComponent } from './leave-policy-component';

describe('LeavePolicyComponent', () => {
  let component: LeavePolicyComponent;
  let fixture: ComponentFixture<LeavePolicyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LeavePolicyComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(LeavePolicyComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
