import { TestBed } from '@angular/core/testing';
import { PublicHolidayFacade } from './public-holiday-facade';

describe('PublicHolidayFacade', () => {
  let service: PublicHolidayFacade;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PublicHolidayFacade);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
