import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssignShift } from './assign-shift';

describe('AssignShift', () => {
  let component: AssignShift;
  let fixture: ComponentFixture<AssignShift>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AssignShift],
    }).compileComponents();

    fixture = TestBed.createComponent(AssignShift);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
