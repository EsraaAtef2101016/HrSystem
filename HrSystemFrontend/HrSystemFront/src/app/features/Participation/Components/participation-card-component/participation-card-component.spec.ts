import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ParticipationCardComponent } from './participation-card-component';

describe('ParticipationCardComponent', () => {
  let component: ParticipationCardComponent;
  let fixture: ComponentFixture<ParticipationCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ParticipationCardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ParticipationCardComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
