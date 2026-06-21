import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Shifts } from './shifts';

describe('Shifts', () => {
  let component: Shifts;
  let fixture: ComponentFixture<Shifts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Shifts],
    }).compileComponents();

    fixture = TestBed.createComponent(Shifts);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
