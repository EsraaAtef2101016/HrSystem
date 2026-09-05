import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EmployeeDashboardDetailsComponent } from './employee-dashboard-details-component';

describe('EmployeeDashboardDetailsComponent', () => {
  let component: EmployeeDashboardDetailsComponent;
  let fixture: ComponentFixture<EmployeeDashboardDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmployeeDashboardDetailsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EmployeeDashboardDetailsComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
